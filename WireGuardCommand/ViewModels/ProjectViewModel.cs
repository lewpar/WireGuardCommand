using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        private WGCConfig? oldConfig;

        [ObservableProperty]
        private bool isClosingWithUnsavedChanges;

        [ObservableProperty]
        private List<TestClient> testClients;

        [ObservableProperty]
        private int maxClients;

        [ObservableProperty]
        private string? serverPreview;

        private bool previewSelected;
        public bool PreviewSelected
        {
            get => previewSelected;
            set
            {
                if(previewSelected != value)
                {
                    previewSelected = value;
                    SetProperty(ref previewSelected, value);

                    if (previewSelected)
                    {
                        _ = LoadPreviewAsync();
                    }
                }
            }
        }

        [ObservableProperty]
        private bool finishedLoading;

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
            if(Config is null)
            {
                Config = new WGCConfig();
            }

            if(Config is not null)
            {
                oldConfig = new WGCConfig()
                {
                    ListenPort = Config.ListenPort,
                    NoOfClients = Config.NoOfClients,
                    Cidr = Config.Cidr,
                    Seed = Config.Seed,
                    AllowedIPs = Config.AllowedIPs,
                    Endpoint = Config.Endpoint,
                    Dns = Config.Dns,
                    PostUpRule = Config.PostUpRule,
                    PostDownRule = Config.PostDownRule
                };
            }
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
            return !(Config!.Equals(oldConfig));
        }

        [RelayCommand]
        private void NewSeed()
        {
            Config!.Seed = RandomHelper.GetRandomSeed();
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

        private async Task LoadPreviewAsync()
        {
            FinishedLoading = false;

            if(Config is null)
            {
                FinishedLoading = true;
                return;
            }

            ServerPreview = Config.GenerateServer();

            FinishedLoading = true;
        }
    }
}
