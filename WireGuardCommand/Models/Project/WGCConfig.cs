using CommunityToolkit.Mvvm.ComponentModel;

using WireGuardCommand.Security;

namespace WireGuardCommand.Models.Project
{
    public partial class WGCConfig : ObservableObject
    {
        [ObservableProperty]
        private int listenPort;

        [ObservableProperty]
        private int noOfClients;

        [ObservableProperty]
        private string? cidr;

        [ObservableProperty]
        private string? seed;

        public WGCConfig()
        {
            ListenPort = 51820;
            NoOfClients = 1;
            Cidr = "10.0.0.0/24";

            Seed = RandomHelper.GetRandomSeed();
        }
    }
}
