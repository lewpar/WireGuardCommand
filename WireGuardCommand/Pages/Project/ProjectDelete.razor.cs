using Microsoft.AspNetCore.Components;

using WireGuardCommand.Services;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectDelete
{
    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    public string? Error { get; set; }

    public void Confirm()
    {
        if(Cache.CurrentProject.Metadata is null)
        {
            return;
        }

        if(string.IsNullOrWhiteSpace(Cache.CurrentProject.Metadata.Path))
        {
            Error = "No project path";
            return;
        }

        try
        {
            Directory.Delete(Cache.CurrentProject.Metadata.Path, true);
        }
        catch (Exception ex)
        {
            Error = $"Failed to delete project: {ex.Message}";
            return;
        }

        NavigationManager.NavigateTo("/");
        Cache.Clear();
    }

    public void Cancel()
    {
        NavigationManager.NavigateTo("/");
        Cache.Clear();
    }
}
