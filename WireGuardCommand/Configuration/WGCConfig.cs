namespace WireGuardCommand.Configuration;

public class WGCConfig
{
    public const string AppSettingsKey = "WireGuardCommand";

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
}
