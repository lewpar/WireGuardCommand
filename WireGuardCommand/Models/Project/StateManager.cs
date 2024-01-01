using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace WireGuardCommand.Models.Project
{
    public partial class StateManager : ObservableObject
    {
        private static StateManager? instance;
        public static StateManager Instance 
        { 
            get
            {
                if(instance is null)
                {
                    instance = new StateManager();
                }

                return instance;
            }
        }

        [ObservableProperty]
        private WGCProject? currentProject;

        [ObservableProperty]
        private WGCConfig? currentConfig;

        public void LoadProject(WGCProject project)
        {
            CurrentProject = project;
        }

        public void LoadConfig()
        {
            if (CurrentProject is null)
            {
                Debug.WriteLine("Failed to deserialize config: CurrentProject is null.");
                return;
            }

            if (string.IsNullOrEmpty(CurrentProject.Path))
            {
                Debug.WriteLine("Failed to deserialize config: CurrentProject Path is null or empty.");
                return;
            }

            try
            {
                var path = Path.Combine(WGCProject.PATH_PROJECTS, CurrentProject.Path, WGCProject.PATH_CONFIG);
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<WGCConfig>(json);

                if(config is null)
                {
                    Debug.WriteLine("Failed to deserialize config: config is null.");
                    return;
                }

                CurrentConfig = config;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to deserialize config: {ex.Message}");
            }
        }
    }
}
