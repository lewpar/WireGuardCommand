using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using WireGuardCommand.Models.Project;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectNavigatorViewModel : ViewModel
    {
        const string PATH_PROJECTS = @"./Projects";

        public ObservableCollection<WGCProject> Projects { get; set; }

        [ObservableProperty]
        private bool projectsLoading;

        [ObservableProperty]
        private bool projectsFound;

        public ProjectNavigatorViewModel()
        {
            Projects = new ObservableCollection<WGCProject>();

            _ = LoadProjectsAsync();
        }

        public async Task LoadProjectsAsync()
        {
            ProjectsLoading = true;

            // Load Project Meta
            {
                foreach (var dir in Directory.GetDirectories(PATH_PROJECTS))
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

                        Projects.Add(projectMeta);

                        ProjectsFound = true;
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Failed to load meta for project '{projectName}': {ex.Message}");
                    }
                }

                // Fake delay for testing loading bar.
                await Task.Delay(1500);

                ProjectsLoading = false;
            }
        }
    }
}
