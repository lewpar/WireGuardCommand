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

    public bool EncryptByDefault { get; set; }

    public WGCConfig()
    {
        ProjectsPath = Path.GetFullPath(".\\Projects");
        EncryptByDefault = false;
    }
}
