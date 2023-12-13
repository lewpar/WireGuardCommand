using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Security;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class ProjectDecryptViewModel : ViewModel
    {
        [ObservableProperty]
        private string? encryptionPhrase;

        [ObservableProperty]
        private string? errorMessage;

        private NavigationService navService;

        [ObservableProperty]
        private WGCProject? project;

        public ProjectDecryptViewModel(NavigationService navService)
        {
            this.navService = navService;
        }

        [RelayCommand]
        private void DecryptProject()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (Project is null)
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project was null.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                if (string.IsNullOrEmpty(Project.Name))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project name was null or empty.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                if (string.IsNullOrEmpty(Project.Path))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project path was null or empty.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                if (Project.Encryption is null)
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project encryption data not found.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                var binaryPath = Path.Combine(Project.Path, "WGC.bin");
                if (!File.Exists(binaryPath))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project encrypted binary not found.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                var data = File.ReadAllBytes(binaryPath);
                if (data.Length <= 0)
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project encrypted binary had no data to decrypt (length was 0).");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                if (string.IsNullOrEmpty(Project.Encryption.Salt))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project encryption salt not found.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                if (string.IsNullOrEmpty(Project.Encryption.IV))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Project encryption IV not found.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                var (key, _) = AESEncryption.GenerateKey(EncryptionPhrase!, Convert.FromBase64String(Project.Encryption.Salt));
                var iv = Convert.FromBase64String(Project.Encryption.IV);

                var result = AESEncryption.TryDecrypt(data, key, iv, out var decryptedData);

                if (!result)
                {
                    Debug.WriteLine("Failed to decrypt project with error: User inputted incorrect passprase or an error occured during decryption.");
                    ErrorMessage = "Invalid passphrase.";
                    return;
                }

                if (decryptedData is null)
                {
                    Debug.WriteLine("Failed to decrypt project with error: Decrypted data was null.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                var json = Encoding.UTF8.GetString(decryptedData);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.WriteLine("Failed to decrypt project with error: Decrypted data was null or empty.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                var wgcConfig = JsonSerializer.Deserialize<WGCConfig>(json);
                if (wgcConfig is null)
                {
                    Debug.WriteLine("Failed to decrypt project with error: Decrypted data failed to be deserialized.");
                    ErrorMessage = "Project metadata is corrupted, seek assistance for recovering data.";
                    return;
                }

                navService.OpenProjectView(Project, wgcConfig);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"An error occured while trying to decrypt a project: {ex.Message}");
                ErrorMessage = "An unexpected error occured.";
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            navService.OpenNavigationView();
        }
    }
}
