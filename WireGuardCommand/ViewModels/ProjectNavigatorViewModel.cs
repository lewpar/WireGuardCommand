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
        public ObservableCollection<WGCProject> Projects { get; set; }

        [ObservableProperty]
        private bool projectsLoaded; // ProjectsLoaded

        [ObservableProperty]
        private bool foundProjects; // FoundProjects

        public ProjectNavigatorViewModel()
        {
            Projects = new ObservableCollection<WGCProject>();
            _ = LoadProjectsAsync();
        }

        public async Task LoadProjectsAsync()
        {
            ProjectsLoaded = false;

            if(!Directory.Exists(@"./Projects"))
            {
                Directory.CreateDirectory(@"./Projects");
            }

            var files = Directory.GetFiles(@"./Projects", "*.wgcp");
            if(files.Length > 0)
            {
                foreach(var file in files)
                {
                    try
                    {
                        using FileStream fs = File.OpenRead(file);
                        var project = await JsonSerializer.DeserializeAsync<WGCProject>(fs);

                        if(project is null)
                        {
                            continue;
                        }

                        if(string.IsNullOrEmpty(project.Name))
                        {
                            project.Name = "Untitled Project";
                        }

                        Projects.Add(project);

                        FoundProjects = true;
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Failed to load WireGuard Command project with exception: {ex.Message}");
                        continue;
                    }
                }
            }

            ProjectsLoaded = true;
        }
    }
}
