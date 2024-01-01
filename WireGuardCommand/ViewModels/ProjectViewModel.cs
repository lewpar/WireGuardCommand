using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Diagnostics;

using WireGuardCommand.Models.Project;
using WireGuardCommand.Security;
using WireGuardCommand.Services;

namespace WireGuardCommand.ViewModels
{
    public partial class TestClient : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string? config;
    }

    public partial class ProjectViewModel : ViewModel
    {
        private readonly NavigationService navService;

        private WGCConfig? oldConfig;

        [ObservableProperty]
        private bool isClosingWithUnsavedChanges;

        [ObservableProperty]
        private ObservableCollection<TestClient> testClients;

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
                        LoadPreview();
                    }
                }
            }
        }

        [ObservableProperty]
        private bool finishedLoading;

        public ProjectViewModel(NavigationService navService) 
        {
            this.navService = navService;

            TestClients = new ObservableCollection<TestClient>();

            MaxClients = 10;
        }

        public override void Load()
        {
            var config = StateManager.Instance.CurrentConfig;
            if (config is not null)
            {
                oldConfig = new WGCConfig()
                {
                    ListenPort = config.ListenPort,
                    NoOfClients = config.NoOfClients,
                    Cidr = config.Cidr,
                    Seed = config.Seed,
                    AllowedIPs = config.AllowedIPs,
                    Endpoint = config.Endpoint,
                    Dns = config.Dns,
                    PostUpRule = config.PostUpRule,
                    PostDownRule = config.PostDownRule
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
            var config = StateManager.Instance.CurrentConfig;

            if(config is null)
            {
                Debug.WriteLine("Failed to assert unsaved changes: config is null.");
                return false;
            }

            return !(config.Equals(oldConfig));
        }

        [RelayCommand]
        private void NewSeed()
        {
            StateManager.Instance.CurrentConfig!.Seed = RandomHelper.GetRandomSeed();
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

        private void LoadPreview()
        {
            FinishedLoading = false;

            var config = StateManager.Instance.CurrentConfig;

            if (config is null)
            {
                FinishedLoading = true;
                return;
            }

            config.GenerateKeyPairs();

            TestClients.Clear();
            for (int i = 1; i < config.NoOfClients + 1; i++)
            {
                TestClients.Add(new TestClient()
                {
                    Id = i,
                    Config = config.GenerateClient(i)
                });
            }

            ServerPreview = config.GenerateServer();

            FinishedLoading = true;
        }
    }
}
