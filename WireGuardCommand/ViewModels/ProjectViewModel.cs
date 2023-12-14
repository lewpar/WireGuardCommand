using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Diagnostics;
using System.Text.Json;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectViewModel : ViewModel
    {
        private readonly NavigationService navService;

        [ObservableProperty]
        private WGCProject? project;

        [ObservableProperty]
        private WGCConfig? config;

        private string? oldJson;

        [ObservableProperty]
        private bool isClosingWithUnsavedChanges;

        public ProjectViewModel(NavigationService navService) 
        {
            this.navService = navService;
        }

        public override void Load()
        {
            oldJson = JsonSerializer.Serialize(Config);
        }

        [RelayCommand]
        private void CloseProject()
        {
            navService.OpenNavigationView();
        }

        [RelayCommand]
        private void TryCloseProject()
        {
            if(!HasUnsavedChanged())
            {
                CloseProject();
                return;
            }

            IsClosingWithUnsavedChanges = true;
        }

        [RelayCommand]
        private void ReturnToProject()
        {
            IsClosingWithUnsavedChanges = false;
        }

        private void SaveProject()
        {

        }

        private bool HasUnsavedChanged()
        {
            var json = JsonSerializer.Serialize(Config);

            return !string.Equals(json, oldJson);
        }

        [RelayCommand]
        private void MakeChanges()
        {
            Debug.WriteLine($"Changed: {HasUnsavedChanged()}");
            Config.Test = "Test123";

            Debug.WriteLine("Made changes.");
            Debug.WriteLine($"Changed: {HasUnsavedChanged()}");
        }
    }
}
