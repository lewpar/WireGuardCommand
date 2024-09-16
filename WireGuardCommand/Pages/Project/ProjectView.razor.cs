using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectView
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;
}
