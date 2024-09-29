namespace WireGuardCommand.Services.Models;

public class ProjectContext
{
    public ProjectMetadata? Metadata { get; set; }

    public ProjectData? ProjectData { get; set; }
    public string? Passphrase { get; set; }

    public ProjectTemplate? CreateTemplate()
    {
        if(Metadata is null ||
            ProjectData is null)
        {
            return null;
        }

        return new ProjectTemplate()
        {
            Name = Metadata.Name ?? $"template-{Guid.NewGuid()}",

            ListenPort = ProjectData.ListenPort,
            NumberOfClients = ProjectData.NumberOfClients,

            Subnet = ProjectData.Subnet,
            AllowedIPs = ProjectData.AllowedIPs,
        };
    }
}
