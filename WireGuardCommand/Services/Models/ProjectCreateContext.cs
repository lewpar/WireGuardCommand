namespace WireGuardCommand.Services.Models;

public class ProjectCreateContext
{
    public string Name { get; set; }
    public string Path { get; set; }

    public bool IsEncrypted { get; set; }
    public string Passphrase { get; set; }

    public ProjectTemplate Template { get; set; }

    public ProjectCreateContext()
    {
        Name = "";
        Path = "";
        IsEncrypted = false;
        Passphrase = "";

        Template = new ProjectTemplate();
    }
}
