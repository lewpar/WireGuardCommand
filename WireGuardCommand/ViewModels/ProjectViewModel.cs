using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Security;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class TestClient : ObservableObject
    {
        [ObservableProperty]
        private int id;
    }

    public partial class ProjectViewModel : ViewModel
    {
        private readonly NavigationService navService;

        [ObservableProperty]
        private WGCProject? project;

        [ObservableProperty]
        private WGCConfig? config;

        private string? oldJson;

        [ObservableProperty]
        private bool isClosingWithUnsavedChanges;

        [ObservableProperty]
        private List<TestClient> testClients;

        [ObservableProperty]
        private int maxClients;

        public ProjectViewModel(NavigationService navService) 
        {
            this.navService = navService;

            TestClients = new List<TestClient>()
            {
                new TestClient()
                {
                    Id = 1
                },
                new TestClient()
                {
                    Id = 2
                }
            };

            MaxClients = 10;
        }

        public override void Load()
        {
            oldJson = JsonSerializer.Serialize(Config);
        }

        [RelayCommand]
        private void CloseProject()
        {
            navService.OpenNavigationView();
        }

        [RelayCommand]
        private void TryCloseProject()
        {
            if(!HasUnsavedChanged())
            {
                CloseProject();
                return;
            }

            IsClosingWithUnsavedChanges = true;
        }

        [RelayCommand]
        private void ReturnToProject()
        {
            IsClosingWithUnsavedChanges = false;
        }

        private void SaveProject()
        {

        }

        private bool HasUnsavedChanged()
        {
            var json = JsonSerializer.Serialize(Config);

            return !string.Equals(json, oldJson);
        }

        [RelayCommand]
        private void NewSeed()
        {
            Config.Seed = RandomHelper.GetRandomSeed();
        }

        [RelayCommand]
        private void GetHelp()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "https://github.com/lewpar/WireGuardCommand",
                UseShellExecute = true
            });
        }
    }
}
