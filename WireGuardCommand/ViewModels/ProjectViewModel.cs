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

        public ProjectViewModel(NavigationService navService) 
        {
            this.navService = navService;
        }

        [RelayCommand]
        private void CloseProject()
        {
            navService.OpenNavigationView();
        }
    }
}
