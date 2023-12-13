using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

        [ObservableProperty]
        private bool isClosingWithUnsavedChanges;

        private bool hasUnsavedChanges;

        public ProjectViewModel(NavigationService navService) 
        {
            this.navService = navService;
        }

        [RelayCommand]
        private void CloseProject()
        {
            navService.OpenNavigationView();
        }

        [RelayCommand]
        private void TryCloseProject()
        {
            if(!hasUnsavedChanges)
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
    }
}
