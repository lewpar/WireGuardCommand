using Microsoft.AspNetCore.Components;

using WireGuardCommand.Components;
using WireGuardCommand.Components.Models;
using WireGuardCommand.Configuration;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectSettings
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public AlertController AlertController { get; set; } = default!;

    [Inject]
    public WGCConfig Config { get; set; } = default!;

    private List<ProjectTemplate> templates = new List<ProjectTemplate>();
    private Dialog dialog = default!;

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
    }

    private async Task SaveAsync()
    {
        try
        {
            await Config.SaveAsync();
            AlertController.Push(AlertType.Info, $"Saved configuration.", 4000);
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to save configuration: {ex.Message}");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        templates = await ProjectManager.GetProjectTemplatesAsync();

        // Default template should not be deletable or shown.
        templates.RemoveAll(t => t.Name.ToLower() == "default");

        StateHasChanged();
    }

    private void DeleteTemplate(ProjectTemplate template)
    {
        dialog.Show(DialogType.YesNo, "Delete Template", $"Are you sure you want to delete the template '{template.Name}'?", async () =>
        {
            await InvokeAsync(() =>
            {
                DeleteTemplateNoPrompt(template);
            });
        });
    }

    private void DeleteTemplateNoPrompt(ProjectTemplate template)
    {
        var path = Path.Combine(Config.TemplatesPath, $"{template.Name.ToLower()}.json");
        if(!File.Exists(path))
        {
            AlertController.Push(AlertType.Error, "Failed to delete template, path not found.");
            return;
        }

        try
        {
            File.Delete(path);
            AlertController.Push(AlertType.Info, $"Delete template '{template.Name}'.", 4000);

            templates.Remove(template);
            StateHasChanged();
        }
        catch(Exception ex)
        {
            AlertController.Push(AlertType.Error, $"Failed to delete template: {ex.Message}");
        }
    }
}
