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

        public void OpenProjectView(WGCProject project, WGCConfig? config = null)
        {
            // Open a project directly in memory
            if(config is not null)
            {
                CurrentView = new ProjectViewModel(this)
                {
                    Project = project,
                    Config = config
                };

                return;
            }

            CurrentView = (project.Encrypted) ?
                new ProjectDecryptViewModel(this)
                {
                    Project = project,
                } :
                new ProjectViewModel(this)
                {
                    Project = project,
                    Config = LoadConfig(project)
                };
        }

        public WGCConfig? LoadConfig(WGCProject project)
        {
            return null;
        }

        public void OpenNewProjectView()
        {
            var newProjView = new ProjectNewViewModel(this);

            CurrentView = newProjView;
        }
    }
}
