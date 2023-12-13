using CommunityToolkit.Mvvm.ComponentModel;

using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class RootViewModel : ViewModel
    {
        [ObservableProperty]
        private NavigationService navService;

        public RootViewModel(NavigationService navService) 
        {
            this.navService = navService;

            navService.OpenNavigationView();
        }
    }
}
