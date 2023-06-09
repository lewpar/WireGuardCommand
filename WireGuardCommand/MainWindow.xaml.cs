using Elliptic;

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

using WireGuardCommand.Models;

namespace WireGuardCommand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LoadDefaults();
        }

        private void LoadDefaults()
        {
            InputPrefix.Text = Properties.Settings.Default.DefaultPrefix;
            InputCommand.Text = Properties.Settings.Default.DefaultCommand;
            InputPostfix.Text = Properties.Settings.Default.DefaultPostfix;

            InputListenPort.Text = Properties.Settings.Default.DefaultListenPort;
            InputNoClients.Text = Properties.Settings.Default.DefaultNoClients;
            InputSubnet.Text = Properties.Settings.Default.DefaultSubnet;

            InputIPs.Text = Properties.Settings.Default.DefaultIPs;
            InputEndpoint.Text = Properties.Settings.Default.DefaultEndpoint;
            InputDNS.Text = Properties.Settings.Default.DefaultDNS;

            InputInterface.Text = Properties.Settings.Default.DefaultInterface;

            CheckBoxPresharedKeys.IsChecked = Properties.Settings.Default.DefaultPresharedKeys;
        }

        private void ButtonSaveDefaults_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DefaultPrefix = InputPrefix.Text;
            Properties.Settings.Default.DefaultCommand = InputCommand.Text;
            Properties.Settings.Default.DefaultPostfix = InputPostfix.Text;

            Properties.Settings.Default.DefaultListenPort = InputListenPort.Text;
            Properties.Settings.Default.DefaultNoClients = InputNoClients.Text;
            Properties.Settings.Default.DefaultSubnet = InputSubnet.Text;

            Properties.Settings.Default.DefaultIPs = InputIPs.Text;
            Properties.Settings.Default.DefaultEndpoint = InputEndpoint.Text;
            Properties.Settings.Default.DefaultDNS = InputDNS.Text;

            Properties.Settings.Default.DefaultInterface = InputInterface.Text;

            Properties.Settings.Default.DefaultPresharedKeys = CheckBoxPresharedKeys.IsChecked.HasValue ? CheckBoxPresharedKeys.IsChecked.Value : false;

            Properties.Settings.Default.Save();
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            string saveLocation = string.Empty;

            if(!string.IsNullOrEmpty(Properties.Settings.Default.SaveLocation))
            {
                saveLocation = Properties.Settings.Default.SaveLocation;
            }

            using (var saveFileDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var saveResult = saveFileDialog.ShowDialog();
                if (saveResult == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.SaveLocation = saveFileDialog.InitialDirectory;
                    Properties.Settings.Default.Save();

                    if(string.IsNullOrEmpty(saveFileDialog.SelectedPath))
                    {
                        MessageBox.Show("Save path is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        var config = CreateServerConfig();
                        if(config == null)
                        {
                            return;
                        }

                        WriteWireGuardConfig(config, saveFileDialog.SelectedPath);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show($"Failed to save files with exception: {ex.GetType()}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show("Successfully generated configs.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ButtonClearDefaults_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            LoadDefaults();
        }

        private WireGuardAddress? ParseAddress()
        {
            var address = new WireGuardAddress();

            var addressCIDR = InputSubnet.Text;
            var match = Regex.Match(addressCIDR, @"([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\.([0-9]{1,3})\/([0-9]{1,2})");

            address.Address = InputSubnet.Text.Split("/")[0];

            if (match.Groups.Count < 6)
            {
                return null;
            }

            try
            {
                address.Octets = new int[]
                {
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value)
                };

                address.CIDR = int.Parse(match.Groups[5].Value);
            }
            catch (Exception)
            { 
                return null;
            }

            return address;
        }

        private WireGuardServer? CreateServerConfig()
        {
            var wgAddress = ParseAddress();
            if (wgAddress == null)
            {
                return null;
            }

            int subnetSize = (int)Math.Pow(2, (32 - wgAddress.CIDR)) - 2; // Subtract network and broadcast address.
            if (subnetSize == 0)
            {
                MessageBox.Show("Subnet size is 0, you must specify a larger CIDR.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var wgServer = new WireGuardServer();

            int listenPort;
            if (!int.TryParse(InputListenPort.Text, out listenPort))
            {
                MessageBox.Show("Failed to get listen port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            wgServer.Port = listenPort;

            int clientCount;
            if (!int.TryParse(InputNoClients.Text, out clientCount))
            {
                MessageBox.Show("Failed to get number of clients.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if(clientCount > subnetSize)
            {
                MessageBox.Show("You cannot have more clients than the subnet can handle.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            // Generate private / public keys.
            byte[] buffer = new byte[32];
            RandomNumberGenerator.Create().GetBytes(buffer);

            byte[] privKey = Curve25519.ClampPrivateKey(buffer);
            byte[] pubKey = Curve25519.GetPublicKey(privKey);

            wgServer.PrivateKey = Convert.ToBase64String(privKey);
            wgServer.PublicKey = Convert.ToBase64String(pubKey);

            wgServer.Interface = InputInterface.Text;

            if(wgAddress.Octets == null)
            {
                MessageBox.Show("Failed to get octets from address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            uint address = (uint)(wgAddress.Octets[0] << 24) + (uint)(wgAddress.Octets[1] << 16) + (uint)(wgAddress.Octets[2] << 8) + (uint)(wgAddress.Octets[3]);
            for (uint i = 1; i < Math.Clamp(subnetSize, 0, clientCount + 2); i++)
            {
                uint newAddress = address + i;

                var newOctets = new uint[]
                {
                    (newAddress >> 24) & 255,
                    (newAddress >> 16) & 255,
                    (newAddress >> 8) & 255,
                    (newAddress) & 255,
                };

                // First address for server.
                if (i == 1)
                {
                    wgServer.Address = $"{newOctets[0]}.{newOctets[1]}.{newOctets[2]}.{newOctets[3]}/{wgAddress.CIDR}";
                }
                else
                {
                    var wgPeer = new WireGuardPeer();
                    wgPeer.Id = (int)i - 1; // Server takes up first id, but peers should start at 1.
                    wgPeer.AllowedIPS = $"{newOctets[0]}.{newOctets[1]}.{newOctets[2]}.{newOctets[3]}/32";

                    // Generate client private / public keys.
                    byte[] clientBuffer = new byte[32];
                    RandomNumberGenerator.Create().GetBytes(clientBuffer);

                    byte[] clientPrivKey = Curve25519.ClampPrivateKey(clientBuffer);
                    byte[] clientPubKey = Curve25519.GetPublicKey(clientPrivKey);

                    wgPeer.PublicKey = Convert.ToBase64String(clientPubKey);

                    wgServer.PrivateKey = Convert.ToBase64String(privKey);
                    wgServer.PublicKey = Convert.ToBase64String(pubKey);

                    var wgClient = new WireGuardClient();
                    wgClient.Address = $"{newOctets[0]}.{newOctets[1]}.{newOctets[2]}.{newOctets[3]}/{wgAddress.CIDR}";
                    wgClient.Port = wgServer.Port;
                    wgClient.AllowedIPS = InputIPs.Text;
                    wgClient.PublicKey = wgServer.PublicKey;
                    wgClient.PrivateKey = Convert.ToBase64String(clientPrivKey);
                    wgClient.DNS = InputDNS.Text;

                    // Generate pre-shared key.
                    if (CheckBoxPresharedKeys.IsChecked.HasValue && CheckBoxPresharedKeys.IsChecked.Value)
                    {
                        byte[] sharedKeyBuffer = new byte[32];
                        RandomNumberGenerator.Create().GetBytes(sharedKeyBuffer);

                        wgPeer.PresharedKey = Convert.ToBase64String(sharedKeyBuffer);
                        wgClient.PresharedKey = Convert.ToBase64String(sharedKeyBuffer);
                    }

                    wgClient.Endpoint = InputEndpoint.Text;

                    wgPeer.Config = wgClient;

                    wgServer.Peers.Add(wgPeer);
                }
            }

            foreach (var peer in wgServer.Peers)
            {
                string command = InputCommand.Text;

                command = command.Replace("{interface}", wgServer.Interface);
                command = command.Replace("{client-address}", peer.AllowedIPS);
                command = command.Replace("{client-id}", peer.Id.ToString());
                command = command.Replace("{client-preshared}", peer.PresharedKey);
                command = command.Replace("{client-public}", peer.PublicKey);

                wgServer.Commands.Add(command);
            }

            return wgServer;
        }

        private void WriteWireGuardConfig(WireGuardServer wgServer, string path)
        {
            var sbServer = new StringBuilder();
            var sbClient = new StringBuilder();

            sbServer.AppendLine("[Interface]");
            sbServer.AppendLine($"Address = {wgServer.Address}");
            sbServer.AppendLine($"ListenPort = {wgServer.Port}");
            sbServer.AppendLine($"PrivateKey = {wgServer.PrivateKey}");

            sbServer.AppendLine("");

            int i = 1;
            foreach (var peer in wgServer.Peers)
            {
                // SERVER
                sbServer.AppendLine($"# Client {i}");
                sbServer.AppendLine("[Peer]");
                sbServer.AppendLine($"PublicKey = {peer.PublicKey}");

                if (!string.IsNullOrEmpty(peer.PresharedKey))
                    sbServer.AppendLine($"PresharedKey = {peer.PresharedKey}");

                sbServer.AppendLine($"AllowedIPs = {peer.AllowedIPS}");

                sbServer.AppendLine("");

                // CLIENT
                sbClient.AppendLine($"# Client {i}");
                sbClient.AppendLine("[Interface]");
                if(peer.Config == null)
                {
                    continue;
                }
                sbClient.AppendLine($"Address = {peer.Config.Address}");
                sbClient.AppendLine($"ListenPort = {peer.Config.Port}");
                sbClient.AppendLine($"PrivateKey = {peer.Config.PrivateKey}");

                if (!string.IsNullOrEmpty(peer.Config.DNS))
                    sbClient.AppendLine($"DNS = {peer.Config.DNS}");

                sbClient.AppendLine("");

                sbClient.AppendLine("[Peer]");
                sbClient.AppendLine($"PublicKey = {peer.Config.PublicKey}");

                if (!string.IsNullOrEmpty(peer.Config.PresharedKey))
                    sbClient.AppendLine($"PresharedKey = {peer.Config.PresharedKey}");

                sbClient.AppendLine($"AllowedIPs = {peer.Config.AllowedIPS}");

                if (!string.IsNullOrEmpty(peer.Config.Endpoint))
                    sbClient.AppendLine($"Endpoint = {peer.Config.Endpoint}");

                File.WriteAllText(Path.Combine(path, $"client{i}.wg"), sbClient.ToString());
                sbClient.Clear();

                i = i + 1;
            }

            File.WriteAllText(Path.Combine(path, "server.wg"), sbServer.ToString());

            var sbCommands = new StringBuilder();

            if (!string.IsNullOrEmpty(InputPrefix.Text))
            {
                sbCommands.AppendLine(InputPrefix.Text);
                sbCommands.AppendLine();
            }

            foreach (var command in wgServer.Commands)
            {
                sbCommands.AppendLine(command);
            }

            if (!string.IsNullOrEmpty(InputPostfix.Text))
            {
                sbCommands.AppendLine(InputPostfix.Text);
                sbCommands.AppendLine();
            }

            File.WriteAllText(Path.Combine(path, "commands.wg"), sbCommands.ToString());
        }

        private void InputSubnet_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var address = ParseAddress();
            if(address == null)
            {
                LabelNoClients.Content = $"No. Clients (No CIDR Detected)";
                return;
            }

            int subnetSize = Math.Clamp((int)Math.Pow(2, (32 - address.CIDR)) - 2, 0, int.MaxValue);
            LabelNoClients.Content = $"No. Clients (Max: {subnetSize})";
        }
    }
}
