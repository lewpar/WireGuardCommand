using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectNavigatorViewModel : ViewModel
    {
        private readonly NavigationService navService;

        public ObservableCollection<WGCProject> Projects { get; set; }

        private WGCProject? selectedProject;
        public WGCProject? SelectedProject
        {
            get { return selectedProject; } 
            set 
            {
                IsProjectSelected = (value is not null);
                SetProperty(ref selectedProject, value);
            }
        }

        [ObservableProperty]
        private bool isProjectSelected;

        [ObservableProperty]
        private bool projectsLoading;

        [ObservableProperty]
        private bool projectsFound;

        public ProjectNavigatorViewModel(NavigationService navService)
        {
            Projects = new ObservableCollection<WGCProject>();

            _ = LoadProjectsAsync();

            this.navService = navService;
        }

        public async Task LoadProjectsAsync()
        {
            ProjectsLoading = true;
            Projects.Clear();

            // Load Project Meta
            {
                foreach (var dir in Directory.GetDirectories(WGCProject.PATH_PROJECTS))
                {
                    var projectName = Path.GetFileName(dir);
                    var pathMeta = Path.Combine(dir, $"{projectName}.meta");

                    if (!File.Exists(pathMeta))
                    {
                        Debug.WriteLine($"No project meta found for project '{projectName}', skipping..");
                        continue;
                    }

                    try
                    {
                        using FileStream fs = File.OpenRead(pathMeta);
                        var projectMeta = await JsonSerializer.DeserializeAsync<WGCProject>(fs);

                        if(projectMeta is null)
                        {
                            throw new Exception($"Failed to deserialize meta information for project '{projectName}'.");
                        }

                        // This should never happen, but just incase?
                        if(string.IsNullOrEmpty(projectMeta.Name))
                        {
                            projectMeta.Name = projectName;
                        }

                        projectMeta.Path = Path.GetFullPath(dir);

                        Projects.Add(projectMeta);

                        ProjectsFound = true;
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Failed to load meta for project '{projectName}': {ex.Message}");
                    }
                }

                // Fake delay for testing loading bar.
                await Task.Delay(500);

                ProjectsLoading = false;
            }
        }

        [RelayCommand]
        private void OpenProject()
        {
            if(SelectedProject is null)
            {
                return;
            }

            Debug.WriteLine($"Open Project: {SelectedProject.Name}");

            navService.OpenProjectView(SelectedProject);
        }

        [RelayCommand]
        private void NewProject()
        {
            navService.OpenNewProjectView();
        }

        [RelayCommand]
        private void DeleteProject()
        {
            if(SelectedProject is null || string.IsNullOrEmpty(SelectedProject.Name))
            {
                return;
            }

            if(!Directory.Exists(Path.Combine(WGCProject.PATH_PROJECTS, SelectedProject.Name)))
            {
                return;
            }

            Directory.Delete(Path.Combine(WGCProject.PATH_PROJECTS, SelectedProject.Name), true);
            Projects.Remove(SelectedProject);
        }
    }
}
