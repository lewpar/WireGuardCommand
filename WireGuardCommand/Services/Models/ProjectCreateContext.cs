namespace WireGuardCommand.Services.Models;

public class ProjectCreateContext
{
    public string Name { get; set; }
    public string Path { get; set; }

    public bool IsEncrypted { get; set; }
    public string Passphrase { get; set; }

    public ProjectCreateContext(string name = "", string path = "", bool isEncrypted = false, string passphrase = "")
    {
        Name = name;
        Path = path;
        IsEncrypted = isEncrypted;
        Passphrase = passphrase;
    }
}
