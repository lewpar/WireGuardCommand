﻿using System.Security.Cryptography;
using System.Text.Json;

using WireGuardCommand.Configuration;
using WireGuardCommand.Extensions;
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
            ProjectData? data;

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

    public async Task SaveProjectAsync(ProjectContext context)
    {
        var metadata = context.Metadata;
        if(metadata is null)
        {
            throw new Exception("Metadata not found for project.");
        }

        var data = context.ProjectData;
        if (data is null)
        {
            throw new Exception("Project data not found for project.");
        }

        var projectPath = metadata.Path;
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            throw new Exception("No path was found for the project.");
        }

        var dataPath = Path.Combine(projectPath, metadata.IsEncrypted ? "data.bin" : "data.json");

        await using (var fs = File.OpenWrite(dataPath))
        {
            fs.SetLength(0);

            if (metadata.IsEncrypted)
            {
                if (string.IsNullOrWhiteSpace(context.Passphrase))
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

                var key = AESProvider.GenerateKey(context.Passphrase, salt);

                using var ms = new MemoryStream();
                await JsonSerializer.SerializeAsync(ms, data);

                var buffer = AESProvider.Encrypt(ms.ToArray(), key, iv);

                await fs.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                await JsonSerializer.SerializeAsync(fs, data, new JsonSerializerOptions()
                {
                    WriteIndented = true
                });
            }
        }
    }

    public async Task<List<ProjectMetadata>> GetProjectsAsync()
    {
        var projects = new List<ProjectMetadata>();
        await using var scope = serviceProvider.CreateAsyncScope();

        var config = scope.ServiceProvider.GetService<WGCConfig>();
        if(config is null)
        {
            throw new Exception("Failed to get configuration.");
        }

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

            ProjectMetadata? metadata;

            try
            {
                metadata = await GetMetadataAsync(metadataPath);
                metadata.Path = directory;
            }
            catch
            {
                metadata = new ProjectMetadata()
                {
                    Name = Path.GetFileName(directory),
                    Path = directory,
                    IsCorrupted = true
                };
            }

            if (projects.Contains(metadata))
            {
                continue;
            }

            projects.Add(metadata);
        }

        return projects;
    }

    private async Task<ProjectMetadata> GetMetadataAsync(string metadataPath)
    {
        await using var fs = File.OpenRead(metadataPath);

        var metadata = await JsonSerializer.DeserializeAsync<ProjectMetadata>(fs);
        if(metadata is null)
        {
            throw new Exception($"Failed to deserialize '{metadataPath}'.");
        }

        return metadata;
    }

    public async Task CreateProjectAsync(ProjectCreateContext createContext)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var config = scope.ServiceProvider.GetService<WGCConfig>();
        if (config is null)
        {
            throw new Exception("Failed to get configuration.");
        }

        if (string.IsNullOrWhiteSpace(createContext.Path))
        {
            throw new Exception("No project path is set.");
        }

        if(string.IsNullOrWhiteSpace(createContext.Name))
        {
            throw new Exception("No project name is set.");
        }

        if(createContext.IsEncrypted &&
            string.IsNullOrWhiteSpace(createContext.Passphrase))
        {
            throw new Exception("No passphrase is set.");
        }

        if (Directory.Exists(createContext.Path) &&
            Directory.GetFiles(createContext.Path).Length != 0)
        {
            throw new Exception("A project already exists at that location.");
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
            await JsonSerializer.SerializeAsync(fs, metadata, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }

        var template = createContext.Template;

        var dataPath = Path.Combine(createContext.Path, metadata.IsEncrypted ? "data.bin" : "data.json");
        var data = new ProjectData()
        {
            Seed = RandomNumberGenerator.GetBytes(config.SeedSize / 8).ToBase64(),

            Interface = template.Interface,

            DNS = template.DNS,
            Endpoint = template.Endpoint,

            ListenPort = template.ListenPort,
            NumberOfClients = template.NumberOfClients,

            Subnet = template.Subnet,
            AllowedIPs = template.AllowedIPs,

            UseLastAddress = template.UseLastAddress,
            UsePresharedKeys = template.UsePresharedKeys,

            PostUp = template.PostUp,
            PostDown = template.PostDown,

            CommandFileName = template.CommandFileName,
            CommandOnce = template.CommandOnce,
            CommandPerPeer = template.CommandPerPeer,
        };

        using (var fs = File.OpenWrite(dataPath))
        {
            if(metadata.IsEncrypted)
            {
                var key = AESProvider.GenerateKey(createContext.Passphrase, metadata.Salt.FromBase64());
                using var ms = new MemoryStream();

                await JsonSerializer.SerializeAsync(ms, data);
                var encryptedData = AESProvider.Encrypt(ms.ToArray(), key, metadata.IV.FromBase64());

                fs.Write(encryptedData, 0, encryptedData.Length);
            }
            else
            {
                await JsonSerializer.SerializeAsync(fs, data, new JsonSerializerOptions()
                {
                    WriteIndented = true 
                });
            }
        }
    }

    public void DeleteProject(ProjectMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Path))
        {
            throw new Exception("No path was found for the project.");
        }

        Directory.Delete(metadata.Path, true);
    }

    public async Task<List<ProjectTemplate>> GetProjectTemplatesAsync()
    {
        var templates = new List<ProjectTemplate>();
        using var scope = serviceProvider.CreateAsyncScope();

        var config = scope.ServiceProvider.GetService<WGCConfig>();
        if (config is null)
        {
            throw new Exception("Failed to get configuration.");
        }

        if (!Directory.Exists(config.TemplatesPath))
        {
            Directory.CreateDirectory(config.TemplatesPath);
        }

        // Default template does not exist on disk.
        templates.Add(new ProjectTemplate());

        var files = Directory.GetFiles(config.TemplatesPath);
        foreach (var file in files)
        {
            using (var fs = File.OpenRead(file))
            {
                var template = await JsonSerializer.DeserializeAsync<ProjectTemplate>(fs);

                if (template is null ||
                    templates.Contains(template))
                {
                    continue;
                }

                templates.Add(template);
            }
        }

        return templates;
    }

    public async Task SaveTemplateAsync(ProjectTemplate template)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var config = scope.ServiceProvider.GetService<WGCConfig>();
        if (config is null)
        {
            throw new Exception("Failed to get configuration.");
        }

        var templatePath = Path.Combine(config.TemplatesPath, $"{template.Name.ToLower()}.json");

        await using (var fs = File.OpenWrite(templatePath))
        {
            // Clear contents
            fs.SetLength(0);

            await JsonSerializer.SerializeAsync(fs, template, new JsonSerializerOptions() 
            { 
                WriteIndented = true 
            });
        }
    }
}
