using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            ErrorMessage = string.Empty;

            var data = File.ReadAllBytes(Path.Combine(WGCProject.PATH_PROJECTS, Project.Name, "WGC.bin"));
            var (key, _) = AESEncryption.GenerateKey(EncryptionPhrase!, Convert.FromBase64String(project.Encryption.Salt));
            var iv = Convert.FromBase64String(project.Encryption.IV);

            var result = AESEncryption.TryDecrypt(data, key, iv, out var decryptedData);

            if(!result)
            {
                ErrorMessage = "Invalid passphrase.";
                return;
            }

            var test = Encoding.UTF8.GetString(decryptedData);

            Debug.WriteLine(test);
        }

        [RelayCommand]
        private void GoBack()
        {
            navService.OpenNavigationView();
        }
    }
}
