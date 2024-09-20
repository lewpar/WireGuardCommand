using System.Text.Json.Serialization;

namespace WireGuardCommand.Services.Models;

public class ProjectContext
{
    public string? Path { get; set; }
    public ProjectMetadata? Metadata { get; set; }

    public ProjectData? ProjectData { get; set; }
    public string? Passphrase { get; set; }
}
