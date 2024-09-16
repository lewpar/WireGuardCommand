namespace WireGuardCommand.Configuration;

public class WGCConfig
{
    public string ProjectsPath { get; set; }
    public bool EncryptByDefault { get; set; }

    public WGCConfig()
    {
        ProjectsPath = "./Projects";
        EncryptByDefault = true;
    }
}
