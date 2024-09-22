using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Services;

public class ProjectCache
{
    public ProjectContext CurrentProject { get; set; } = new ProjectContext();

    public void Clear()
    {
        CurrentProject = new ProjectContext();
        GC.Collect();
    }
}
