using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text.Json;

using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
using WireGuardCommand.Models;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Services;

public class ProjectManager
{
    public List<ProjectMetadata> Projects { get; private set; }

    public ProjectContext CurrentProject { get; set; }

    private readonly IServiceProvider serviceProvider;

    public ProjectManager(IServiceProvider serviceProvider)
    {
        CurrentProject = new ProjectContext();

        Projects = new List<ProjectMetadata>();
        this.serviceProvider = serviceProvider;
    }

    public async Task LoadProjectAsync()
    {
        if(CurrentProject is null)
        {
            return;
        }

        if(string.IsNullOrWhiteSpace(CurrentProject.Path))
        {
            return;
        }

        var dataPath = Path.Combine(CurrentProject.Path, "data.json");
        using(var fs = File.OpenRead(dataPath))
        {
            var data = await JsonSerializer.DeserializeAsync<ProjectData>(fs);
            CurrentProject.ProjectData = data;
        }
    }

    public async Task LoadProjectsAsync()
    {
        Projects.Clear();

        using var scope = serviceProvider.CreateAsyncScope();

        var options = scope.ServiceProvider.GetService<IOptions<WGCConfig>>();
        if(options is null)
        {
            throw new Exception("Failed to get configuration service.");
        }

        var config = options.Value;

        if(!Directory.Exists(config.ProjectsPath))
        {
            Directory.CreateDirectory(config.ProjectsPath);
        }

        var projects = Directory.GetDirectories(config.ProjectsPath);
        foreach(var project in projects)
        {
            var metadataPath = Path.Combine(project, "project.json");
            if(!File.Exists(metadataPath))
            {
                continue;
            }

            var result = await TryLoadMetadataAsync(metadataPath);
            if(!result.Success)
            {
                throw new Exception(result.Message);
            }

            if(result.Result is null)
            {
                throw new NullReferenceException("Metadata result in null.");
            }

            var metadata = result.Result;

            if(Projects.Contains(metadata))
            {
                continue;
            }

            Projects.Add(metadata);
        }
    }

    private async Task<ActionResult<ProjectMetadata>> TryLoadMetadataAsync(string metadataPath)
    {
        using var fs = File.OpenRead(metadataPath);

        var metadata = await JsonSerializer.DeserializeAsync<ProjectMetadata>(fs);
        if(metadata is null)
        {
            return new ActionResult<ProjectMetadata>(false, $"Failed to deserialize '{metadataPath}'.");
        }

        var path = Path.GetFullPath(metadataPath);

        metadata.Path = path;
        CurrentProject.Path = Path.GetDirectoryName(path);

        return new ActionResult<ProjectMetadata>(true, result: metadata);
    }

    public async Task<ActionResult<ProjectData>> TryDecryptDataAsync()
    {
        var metadata = CurrentProject.Metadata;
        if(metadata is null)
        {
            return new ActionResult<ProjectData>(false, "No project is currently loaded.");
        }

        if(string.IsNullOrWhiteSpace(CurrentProject.Path))
        {
            return new ActionResult<ProjectData>(false, "No path was set for current project.");
        }

        if (CurrentProject.Passphrase is null || 
            CurrentProject.Passphrase.Length == 0)
        {
            return new ActionResult<ProjectData>(false, "No passphrase was set.");
        }

        var path = Path.Combine(CurrentProject.Path, "project.bin");
        if(!File.Exists(path))
        {
            return new ActionResult<ProjectData>(false, "No project data found.");
        }

        var data = await File.ReadAllBytesAsync(CurrentProject.Path);

        return new ActionResult<ProjectData>(true);
    }

    public async Task<ActionResult> TryCreateProjectAsync(ProjectCreateContext createContext)
    {
        try
        {
            if(string.IsNullOrWhiteSpace(createContext.Path))
            {
                return ActionResult.Fail("No project path is set.");
            }

            if(string.IsNullOrWhiteSpace(createContext.Name))
            {
                return ActionResult.Fail("No project name is set.");
            }

            if(createContext.IsEncrypted &&
                string.IsNullOrWhiteSpace(createContext.Passphrase))
            {
                return ActionResult.Fail("No passphrase is set.");
            }

            if (Directory.Exists(createContext.Path) &&
                Directory.GetFiles(createContext.Path).Length != 0)
            {
                return ActionResult.Fail("A project already exists at this path.");
            }

            Directory.CreateDirectory(createContext.Path);

            var metadataPath = Path.Combine(createContext.Path, "project.json");
            var metadata = new ProjectMetadata()
            {
                Name = createContext.Name,
                IsEncrypted = createContext.IsEncrypted,
                Path = metadataPath
            };

            using (var fs = File.OpenWrite(metadataPath))
            {
                await JsonSerializer.SerializeAsync<ProjectMetadata>(fs, metadata);
            }

            var dataPath = Path.Combine(createContext.Path, "data.json");
            var data = new ProjectData()
            {
                Seed = RandomNumberGenerator.GetBytes(32).ToBase64()
            };

            using (var fs = File.OpenWrite(dataPath))
            {
                await JsonSerializer.SerializeAsync<ProjectData>(fs, data);
            }
        }
        catch(Exception ex)
        {
            return ActionResult.Fail($"{ex.Message}");
        }

        return ActionResult.Pass();
    }
}
