using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WireGuardCommand.Models.Project;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectNewViewModel : ViewModel
    {
        private readonly RootViewModel rootViewModel;

        [ObservableProperty]
        private string? projectName;

        [ObservableProperty]
        private string? projectDescription;

        [ObservableProperty]
        private bool isEncrypted;

        [ObservableProperty]
        private string? encryptionPhrase;

        [ObservableProperty]
        private string? errorMessage;

        public ProjectNewViewModel(RootViewModel rootViewModel) 
        {
            this.rootViewModel = rootViewModel;
        }

        [RelayCommand]
        private void GoBack()
        {
            rootViewModel.ChangeViewModel(rootViewModel.ProjectNavigatorViewModel);
        }

        [RelayCommand]
        private void CreateProject()
        {
            ErrorMessage = string.Empty;

            if(string.IsNullOrEmpty(ProjectName))
            {
                ErrorMessage = "You must supply a project name.";
                return;
            }

            if(ProjectExists(ProjectName))
            {
                ErrorMessage = "A project with that name already exists!";
                return;
            }

            if(IsEncrypted && string.IsNullOrEmpty(EncryptionPhrase))
            {
                ErrorMessage = "You must supply an encryption phrase.";
                return;
            }

            CreateProject(new WGCProject()
            {
                Name = ProjectName,
                Description = ProjectDescription,
                Encrypted = IsEncrypted
            });

            _ = rootViewModel.ProjectNavigatorViewModel.LoadProjectsAsync();
            rootViewModel.ChangeViewModel(rootViewModel.ProjectNavigatorViewModel);
            ResetFields();
        }

        private bool ProjectExists(string projectName)
        {
            return Directory.Exists(Path.Combine(WGCProject.PATH_PROJECTS, projectName));
        }

        private void CreateProject(WGCProject project)
        {
            if(string.IsNullOrEmpty(project.Name))
            {
                Debug.WriteLine("An error ocurred while trying to create a new WireGuard Command Project: Name was null or empty.");
                return;
            }

            Directory.CreateDirectory(Path.Combine(WGCProject.PATH_PROJECTS, project.Name));

            using var fs = File.OpenWrite(Path.Combine(WGCProject.PATH_PROJECTS, project.Name, $"{project.Name}.meta"));
            JsonSerializer.Serialize(fs, project);
        }

        private void ResetFields()
        {
            ErrorMessage = "";
            IsEncrypted = false;
            EncryptionPhrase = string.Empty;
            ProjectName = string.Empty;
            ProjectDescription = string.Empty;
        }
    }
}
