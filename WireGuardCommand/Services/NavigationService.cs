using CommunityToolkit.Mvvm.ComponentModel;
using WireGuardCommand.Models.Project;
using WireGuardCommand.ViewModels;

namespace WireGuardCommand.Services
{
    public partial class NavigationService : ObservableObject
    {
        [ObservableProperty]
        private ViewModel? currentView;

        public void OpenNavigationView()
        {
            var navView = new ProjectNavigatorViewModel(this);

            CurrentView = navView;
        }

        public void OpenProjectView(WGCProject project)
        {
            var projView = new ProjectViewModel(this);
            projView.Project = project;

            CurrentView = projView;
        }

        public void OpenNewProjectView()
        {
            var newProjView = new ProjectNewViewModel(this);

            CurrentView = newProjView;
        }
    }
}
