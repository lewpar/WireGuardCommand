using System.IO;
using System.Text.Json;

namespace WireGuardCommand.Configuration;

public class WGCConfig
{
    private string? projectsPath;
    public string ProjectsPath
    {
        get
        {
            return projectsPath ?? "";
        }
        set
        {
            projectsPath = Path.GetFullPath(value);
        }
    }

    private string? templatesPath;
    public string TemplatesPath
    {
        get
        {
            return templatesPath ?? "";
        }
        set
        {
            templatesPath = Path.GetFullPath(value);
        }
    }

    public bool EncryptByDefault { get; set; }
    public int SeedSize { get; set; }

    public WGCConfig()
    {
        ProjectsPath = Path.GetFullPath(".\\Projects");
        TemplatesPath = Path.GetFullPath(".\\Templates");
        EncryptByDefault = false;
        SeedSize = 2048;
    }

    public async Task SaveAsync()
    {
        var path = Path.Combine("./", "wgc.json");

        using (var fs = File.OpenWrite(path))
        {
            await JsonSerializer.SerializeAsync<WGCConfig>(fs, this, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }
    }
}
