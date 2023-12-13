using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Security;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectNewViewModel : ViewModel
    {
        private readonly NavigationService navService;

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

        public ProjectNewViewModel(NavigationService navService) 
        {
            this.navService = navService;
        }

        [RelayCommand]
        private void GoBack()
        {
            navService.OpenNavigationView();
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

            if(CheckProjectExists(ProjectName))
            {
                ErrorMessage = "A project with that name already exists!";
                return;
            }

            if(IsEncrypted && string.IsNullOrEmpty(EncryptionPhrase))
            {
                ErrorMessage = "You must supply an encryption phrase.";
                return;
            }

            var newProject = new WGCProject()
            {
                Name = ProjectName,
                Path = Path.Combine(WGCProject.PATH_PROJECTS, ProjectName),
                Description = ProjectDescription,
                Encrypted = IsEncrypted
            };

            if(IsEncrypted)
            {
                newProject.Encryption = new WGCEncryption()
                {
                    IV = Convert.ToBase64String(AESEncryption.GenerateIV())
                };
            }

            CreateProject(newProject);

            navService.OpenProjectView(newProject);

            ResetFields();
        }

        private bool CheckProjectExists(string projectName)
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

            if (IsEncrypted)
            {
                CreateEncryptedWireGuardConfig(project, EncryptionPhrase!);
            }
            else
            {
                CreateWireGuardConfig(project);
            }

            using var fs = File.OpenWrite(Path.Combine(WGCProject.PATH_PROJECTS, project.Name, $"{project.Name}.meta"));
            JsonSerializer.Serialize(fs, project);
        }

        private void CreateWireGuardConfig(WGCProject project)
        {
            if(string.IsNullOrEmpty(ProjectName))
            {
                return;
            }

            File.WriteAllText(Path.Combine(WGCProject.PATH_PROJECTS, ProjectName, "WGC.json"), JsonSerializer.Serialize<WGCConfig>(new WGCConfig()));
        }

        private void CreateEncryptedWireGuardConfig(WGCProject project, string phrase)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return;
            }

            if(project is null || 
                project.Encryption is null ||
                string.IsNullOrEmpty(project.Encryption.IV))
            {
                return;
            }

            var iv = Convert.FromBase64String(project.Encryption.IV);
            var (key, salt) = AESEncryption.GenerateKey(EncryptionPhrase!);

            project.Encryption.Salt = Convert.ToBase64String(salt);

            var json = JsonSerializer.Serialize(new WGCConfig());
            var data = Encoding.UTF8.GetBytes(json);
            var encryptedData = AESEncryption.Encrypt(data, key, iv);

            File.WriteAllBytes(Path.Combine(WGCProject.PATH_PROJECTS, ProjectName, "WGC.bin"), encryptedData);
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
