using CommunityToolkit.Mvvm.ComponentModel;

using System.Diagnostics;
using System.IO;
using System.Text.Json;

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
            StateManager.Instance.LoadProject(project);

            CurrentView = (project.Encrypted) ?
                new ProjectDecryptViewModel(this) :
                new ProjectViewModel(this);

            // Load the config immedietly if its not encrypted.
            if(!project.Encrypted)
            {
                StateManager.Instance.LoadConfig();
            }

            CurrentView.Load();
        }

        public void OpenNewProjectView()
        {
            var newProjView = new ProjectNewViewModel(this);

            CurrentView = newProjView;
        }
    }
}
