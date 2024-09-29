using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using WireGuardCommand.Components;
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
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    public IOptions<WGCConfig> Options { get; set; } = default!;

    public ProjectCreateContext CreateContext { get; set; } = default!;
    public List<ProjectTemplate> Templates { get; set; } = new List<ProjectTemplate>();

    public string? SelectedTemplateName { get; set; }

    protected override void OnParametersSet()
    {
        var config = Options.Value;

        CreateContext = new ProjectCreateContext()
        {
            Name = "New Project",
            Path = Path.Combine(config.ProjectsPath, "New Project"),
            IsEncrypted = config.EncryptByDefault
        };

        StateHasChanged();
    }

    private async Task CreateProjectAsync()
    {
        try
        {
            ProjectTemplate? template = null;
            if(!string.IsNullOrWhiteSpace(SelectedTemplateName))
            {
                template = Templates.FirstOrDefault(t => t.Name == SelectedTemplateName);
            }

            if(string.IsNullOrWhiteSpace(SelectedTemplateName) ||
                template is null)
            {
                template = new ProjectTemplate();
            }

            if(template is null)
            {
                AlertController.Push(AlertType.Error, "Failed to load template.");
                return;
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

        var config = Options.Value;

        CreateContext.Name = name;
        CreateContext.Path = Path.Combine(config.ProjectsPath, name);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Templates = await ProjectManager.GetProjectTemplatesAsync();
        StateHasChanged();
    }
}
