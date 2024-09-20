using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using WireGuardCommand.Configuration;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectCreate
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    public IOptions<WGCConfig> Options { get; set; } = default!;

    public ProjectCreateContext CreateContext { get; set; } = default!;

    public string? Error { get; set; }

    protected override void OnParametersSet()
    {
        var config = Options.Value;

        CreateContext = new ProjectCreateContext(name: "New Project", 
            path: Path.Combine(config.ProjectsPath, "New Project"), 
            isEncrypted: config.EncryptByDefault);

        StateHasChanged();
    }

    private async Task CreateProjectAsync()
    {
        var result = await ProjectManager.TryCreateProjectAsync(CreateContext);
        if(!result.Success)
        {
            Error = result.Message;
            return;
        }

        NavigationManager.NavigateTo("/");
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }

    private void OnProjectNameChanged(ChangeEventArgs e)
    {
        if(e.Value is not string)
        {
            return;
        }

        var name = e.Value.ToString();
        if(name is null)
        {
            return;
        }

        var config = Options.Value;

        CreateContext.Name = name;
        CreateContext.Path = Path.Combine(config.ProjectsPath, name);
    }
}
