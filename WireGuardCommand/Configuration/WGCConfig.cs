using System.Text.Json;

namespace WireGuardCommand.Configuration;

public class WGCConfig
{
    public string ProjectsPath { get; set; }
    public string TemplatesPath { get; set; }

    public bool EncryptByDefault { get; set; }
    public int SeedSize { get; set; }

    public WGCConfig()
    {
        ProjectsPath = ".\\Projects";
        TemplatesPath = ".\\Templates";
        EncryptByDefault = false;
        SeedSize = 2048;
    }

    public async Task SaveAsync()
    {
        var path = Path.Combine("./", "wgc.json");

        using (var fs = File.OpenWrite(path))
        {
            fs.SetLength(0);

            await JsonSerializer.SerializeAsync(fs, this, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }
    }
}
