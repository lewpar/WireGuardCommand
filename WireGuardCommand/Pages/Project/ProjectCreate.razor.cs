using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components.Models;
using WireGuardCommand.Configuration;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectCreate
{
    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public WGCConfig Config { get; set; } = default!;

    public ProjectCreateContext CreateContext { get; set; } = default!;
    public List<ProjectTemplate> Templates { get; set; } = new List<ProjectTemplate>();

    private string? selectedTemplateName;

    protected override void OnParametersSet()
    {
        CreateContext = new ProjectCreateContext()
        {
            Name = "New Project",
            Path = Path.GetFullPath(Path.Combine(Config.ProjectsPath, "New Project")),
            IsEncrypted = Config.EncryptByDefault
        };

        StateHasChanged();
    }

    private async Task CreateProjectAsync()
    {
        try
        {
            ProjectTemplate? template = null;
            if(!string.IsNullOrWhiteSpace(selectedTemplateName))
            {
                template = Templates.FirstOrDefault(t => t.Name == selectedTemplateName);
            }

            if(string.IsNullOrWhiteSpace(selectedTemplateName) ||
                template is null)
            {
                template = new ProjectTemplate();
            }

            CreateContext.Template = template;

            await ProjectManager.CreateProjectAsync(CreateContext);
            NavigationManager.NavigateTo("/");
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to create project: {ex.Message}");
        }
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

        CreateContext.Name = name;
        CreateContext.Path = Path.GetFullPath(Path.Combine(Config.ProjectsPath, name));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Templates = await ProjectManager.GetProjectTemplatesAsync();
        StateHasChanged();
    }
}
