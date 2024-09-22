namespace WireGuardCommand.Services.Models;

public class ProjectContext
{
    public ProjectMetadata? Metadata { get; set; }

    public ProjectData? ProjectData { get; set; }
    public string? Passphrase { get; set; }
}
