using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

using WireGuardCommand.Models.Project;
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

            var newProject = new WGCProject()
            {
                Name = ProjectName,
                Description = ProjectDescription,
                Encrypted = IsEncrypted
            };
            CreateProject(newProject);

            navService.OpenProjectView(newProject);

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

            CreateWireGuardConfig();

            if(project.Encrypted)
            {
                if(string.IsNullOrEmpty(EncryptionPhrase))
                {
                    Debug.WriteLine("An error occured while trying to encrypt WireGuard Command Config: Encryption Phrase was null or empty.");
                    return;
                }

                EncryptWireGuardConfig(EncryptionPhrase);
            }
        }

        private void CreateWireGuardConfig()
        {
            if(string.IsNullOrEmpty(ProjectName))
            {
                return;
            }

            File.WriteAllText(Path.Combine(WGCProject.PATH_PROJECTS, ProjectName, "WGC.json"), JsonSerializer.Serialize<WGCConfig>(new WGCConfig())); ;
        }

        private void EncryptWireGuardConfig(string phrase)
        {
            if (string.IsNullOrEmpty(ProjectName))
            {
                return;
            }

            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            var key = aes.Key;
            var iv = aes.IV;

            var message = File.ReadAllText(Path.Combine(WGCProject.PATH_PROJECTS, ProjectName, "WGC.json"));

            string encrypted = Encrypt(message, key, iv);
            File.WriteAllText(Path.Combine(WGCProject.PATH_PROJECTS, ProjectName, "WGC.json"), encrypted);
        }

        private string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
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
