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

        public void OpenProjectView(WGCProject project, WGCConfig? config = null)
        {
            StateManager.Instance.SetProject(project);

            // Open a project directly in memory
            if(config is not null)
            {
                CurrentView = new ProjectViewModel(this)
                {
                    Config = config
                };

                CurrentView.Load();

                return;
            }

            CurrentView = (project.Encrypted) ?
                new ProjectDecryptViewModel(this) :
                new ProjectViewModel(this)
                {
                    Config = LoadConfig()
                };

            CurrentView.Load();
        }

        public WGCConfig? LoadConfig()
        {
            if(StateManager.Instance.CurrentProject is null)
            {
                Debug.WriteLine("CurrentProject is null.");
                return null;
            }

            if(string.IsNullOrEmpty(StateManager.Instance.CurrentProject.Path))
            {
                Debug.WriteLine("CurrentProject Path is null or empty.");
                return null;
            }

            var path = Path.Combine(WGCProject.PATH_PROJECTS, StateManager.Instance.CurrentProject.Path, "wgc.json");
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<WGCConfig>(json);

            return config;
        }

        public void OpenNewProjectView()
        {
            var newProjView = new ProjectNewViewModel(this);

            CurrentView = newProjView;
        }
    }
}
