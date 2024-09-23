using Microsoft.AspNetCore.Components;

using System.Security.Cryptography;
using System.Text.Json;

using WireGuardCommand.Extensions;
using WireGuardCommand.Services;
using WireGuardCommand.Services.Models;

namespace WireGuardCommand.Pages.Project;

public partial class ProjectView
{
    [Inject]
    public ProjectManager ProjectManager { get; set; } = default!;

    [Inject]
    public ProjectCache Cache { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ILogger<ProjectView> Logger { get; set; } = default!;

    public string? Error { get; set; }

    public bool HasUnsavedChanges
    {
        get => HasChanges();
    }

    private ProjectData? originalData;

    protected override void OnInitialized()
    {
        if(Cache.CurrentProject.ProjectData is null)
        {
            return;
        }

        originalData = Cache.CurrentProject.ProjectData.Copy();
        Logger.LogInformation("Loaded project.");
    }

    private void GoBack()
    {
        NavigationManager.NavigateTo("/");
        Cache.Clear();
    }

    private void GenerateSeed()
    {
        var project = Cache.CurrentProject;
        if(project.ProjectData is null)
        {
            Error = "Failed to generate seed.";
            return;
        }

        project.ProjectData.Seed = RandomNumberGenerator.GetBytes(32).ToBase64();
    }

    private bool HasChanges()
    {
        if(originalData is null || 
            Cache.CurrentProject.ProjectData is null)
        {
            Error = "Unable to determine changes.";
            return false;
        }

        return JsonSerializer.Serialize(originalData) != JsonSerializer.Serialize(Cache.CurrentProject.ProjectData);
    }

    public async Task SaveChangesAsync()
    {
        if(Cache.CurrentProject.ProjectData is null)
        {
            return;
        }

        originalData = Cache.CurrentProject.ProjectData.Copy();
        StateHasChanged();
    }
}
