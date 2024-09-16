using Microsoft.Extensions.Options;

using System.Text.Json;

using WireGuardCommand.Configuration;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Services;

public class ProjectManager
{
    public List<ProjectMetadata> Projects { get; private set; }
    public ProjectMetadata? CurrentProject { get; set; }

    private readonly IServiceProvider serviceProvider;

    public ProjectManager(IServiceProvider serviceProvider)
    {
        Projects = new List<ProjectMetadata>();
        this.serviceProvider = serviceProvider;
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

            var metadata = await LoadMetadataAsync(metadataPath);
            if(Projects.Contains(metadata))
            {
                continue;
            }

            Projects.Add(metadata);
        }
    }

    private async Task<ProjectMetadata> LoadMetadataAsync(string metadataPath)
    {
        using var fs = File.OpenRead(metadataPath);

        var metadata = await JsonSerializer.DeserializeAsync<ProjectMetadata>(fs);
        if(metadata is null)
        {
            throw new Exception($"Failed to deserialize '{metadataPath}'.");
        }

        metadata.Path = Path.GetFullPath(metadataPath);

        return metadata;
    }
}
