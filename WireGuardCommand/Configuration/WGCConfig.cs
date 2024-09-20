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

    public bool EncryptByDefault { get; set; }

    public WGCConfig()
    {
        ProjectsPath = Path.GetFullPath(".\\Projects");
        EncryptByDefault = true;
    }
}
