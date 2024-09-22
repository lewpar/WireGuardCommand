using Microsoft.Extensions.Options;

using System.Security.Cryptography;
using System.Text.Json;

using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
using WireGuardCommand.Models;
using WireGuardCommand.Security;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Services;

public class ProjectManager
{
    private readonly IServiceProvider serviceProvider;

    public ProjectManager(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<ProjectData> LoadProjectAsync(ProjectMetadata metadata, string passphrase = "")
    {
        var projectPath = metadata.Path;
        if(string.IsNullOrWhiteSpace(projectPath))
        {
            throw new Exception("No path was found for the project.");
        }

        var dataPath = Path.Combine(projectPath, metadata.IsEncrypted ? "data.bin" : "data.json");

        using (var fs = File.OpenRead(dataPath))
        {
            ProjectData? data = null;

            if (!metadata.IsEncrypted)
            {
                data = await JsonSerializer.DeserializeAsync<ProjectData>(fs);
                if(data is null)
                {
                    throw new Exception("Failed to deserialize project data.");
                }

                return data;
            }
            else
            { 
                if (string.IsNullOrWhiteSpace(passphrase))
                {
                    throw new Exception("No passphrase found for current project.");
                }

                if (string.IsNullOrEmpty(metadata.Salt))
                {
                    throw new Exception("Failed to retrieve Salt from metadata.");
                }

                if (string.IsNullOrEmpty(metadata.IV))
                {
                    throw new Exception("Failed to retrieve IV from metadata.");
                }

                var salt = metadata.Salt.FromBase64();
                var iv = metadata.IV.FromBase64();

                byte[]? buffer = AESProvider.Decrypt(fs, AESProvider.GenerateKey(passphrase, salt), iv);
                if(buffer is null)
                {
                    throw new Exception("Failed to decrypt project.");
                }

                using var ms = new MemoryStream(buffer);

                data = await JsonSerializer.DeserializeAsync<ProjectData>(ms);
                if(data is null)
                {
                    throw new Exception("Failed to deserialize project data.");
                }

                return data;
            }
        }
    }

    public async Task<List<ProjectMetadata>> GetProjectsAsync()
    {
        var projects = new List<ProjectMetadata>();
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

        var directories = Directory.GetDirectories(config.ProjectsPath);
        foreach(var directory in directories)
        {
            var metadataPath = Path.Combine(directory, "project.json");
            if(!File.Exists(metadataPath))
            {
                continue;
            }

            var metadata = await GetMetadataAsync(metadataPath);
            metadata.Path = directory;

            if(projects.Contains(metadata))
            {
                continue;
            }

            projects.Add(metadata);
        }

        return projects;
    }

    private async Task<ProjectMetadata> GetMetadataAsync(string metadataPath)
    {
        using var fs = File.OpenRead(metadataPath);

        var metadata = await JsonSerializer.DeserializeAsync<ProjectMetadata>(fs);
        if(metadata is null)
        {
            throw new Exception($"Failed to deserialize '{metadataPath}'.");
        }

        return metadata;
    }

    public async Task CreateProjectAsync(ProjectCreateContext createContext)
    {
        if(string.IsNullOrWhiteSpace(createContext.Path))
        {
            new Exception("No project path is set.");
        }

        if(string.IsNullOrWhiteSpace(createContext.Name))
        {
            new Exception("No project name is set.");
        }

        if(createContext.IsEncrypted &&
            string.IsNullOrWhiteSpace(createContext.Passphrase))
        {
            new Exception("No passphrase is set.");
        }

        if (Directory.Exists(createContext.Path) &&
            Directory.GetFiles(createContext.Path).Length != 0)
        {
            new Exception("A project already exists at this path.");
        }

        Directory.CreateDirectory(createContext.Path);

        var metadataPath = Path.Combine(createContext.Path, "project.json");
        var metadata = new ProjectMetadata()
        {
            Name = createContext.Name,
            IsEncrypted = createContext.IsEncrypted,
            Path = metadataPath,
            Salt = RandomNumberGenerator.GetBytes(32).ToBase64(),
            IV = AESProvider.GenerateIV().ToBase64()
        };

        using (var fs = File.OpenWrite(metadataPath))
        {
            await JsonSerializer.SerializeAsync<ProjectMetadata>(fs, metadata);
        }

        var dataPath = Path.Combine(createContext.Path, metadata.IsEncrypted ? "data.bin" : "data.json");
        var data = new ProjectData()
        {
            Seed = RandomNumberGenerator.GetBytes(32).ToBase64()
        };

        using (var fs = File.OpenWrite(dataPath))
        {
            if(metadata.IsEncrypted)
            {
                var key = AESProvider.GenerateKey(createContext.Passphrase, metadata.Salt.FromBase64());
                using var ms = new MemoryStream();

                await JsonSerializer.SerializeAsync<ProjectData>(ms, data);
                var encryptedData = AESProvider.Encrypt(ms.ToArray(), key, metadata.IV.FromBase64());

                fs.Write(encryptedData, 0, encryptedData.Length);
            }
            else
            {
                await JsonSerializer.SerializeAsync<ProjectData>(fs, data);
            }
        }
    }
}
