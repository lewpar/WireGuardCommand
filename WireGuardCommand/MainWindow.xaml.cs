﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        private AppSettings _settings;

        public MainWindow(AppSettings settings)
        {
            _settings = settings;

            InitializeComponent();

            if(_settings != null)
                LoadDefaults();
        }

        private void LoadDefaults()
        {
            InputPrefix.Text = _settings.DefaultPrefix;
            InputCommand.Text = _settings.DefaultCommand;
            InputPostfix.Text = _settings.DefaultPostfix;

            InputListenPort.Text = _settings.DefaultListenPort.ToString();
            InputNoClients.Text = _settings.DefaultNoClients.ToString();
            InputSubnet.Text = _settings.DefaultSubnet;

            InputIPs.Text = _settings.DefaultIPs;
            InputEndpoint.Text = _settings.DefaultEndpoint;
            InputDNS.Text = _settings.DefaultDNS;

            InputInterface.Text = _settings.DefaultInterface;

            CheckBoxPresharedKeys.IsChecked = _settings.DefaultPresharedKeys;
            CheckBoxSaveToZip.IsChecked = _settings.DefaultSaveToZip;
            CheckBoxAssignLastIP.IsChecked = _settings.DefaultAssignLastIP;
        }

        private void ButtonSaveDefaults_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(InputPrefix.Text))
            {
                _settings.DefaultPrefix = InputPrefix.Text;
            }

            if (!string.IsNullOrEmpty(InputCommand.Text))
            {
                _settings.DefaultCommand = InputCommand.Text;
            }

            if (!string.IsNullOrEmpty(InputPostfix.Text))
            {
                _settings.DefaultPostfix = InputPostfix.Text;
            }

            if (int.TryParse(InputListenPort.Text, out int port))
            {
                _settings.DefaultListenPort = port;
            }

            if (int.TryParse(InputNoClients.Text, out int num))
            {
                _settings.DefaultNoClients = num;
            }

            if (!string.IsNullOrEmpty(InputSubnet.Text))
            {
                _settings.DefaultSubnet = InputSubnet.Text;
            }

            if (!string.IsNullOrEmpty(InputIPs.Text))
            {
                _settings.DefaultIPs = InputIPs.Text;
            }

            if (!string.IsNullOrEmpty(InputEndpoint.Text))
            {
                _settings.DefaultEndpoint = InputEndpoint.Text;
            }

            if (!string.IsNullOrEmpty(InputDNS.Text))
            {
                _settings.DefaultDNS = InputDNS.Text;
            }

            if (!string.IsNullOrEmpty(InputInterface.Text))
            {
                _settings.DefaultInterface = InputInterface.Text;
            }

            if (CheckBoxPresharedKeys.IsChecked.HasValue)
            {
                _settings.DefaultPresharedKeys = CheckBoxPresharedKeys.IsChecked.Value;
            }

            if (CheckBoxSaveToZip.IsChecked.HasValue)
            {
                _settings.DefaultSaveToZip = CheckBoxSaveToZip.IsChecked.Value;
            }

            if (CheckBoxAssignLastIP.IsChecked.HasValue)
            {
                _settings.DefaultAssignLastIP = CheckBoxAssignLastIP.IsChecked.Value;
            }

            _settings.Save();
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            string saveLocation = string.Empty;

            if(!string.IsNullOrEmpty(_settings.SaveLocation))
            {
                saveLocation = _settings.SaveLocation;
            }

            try 
            { 
                var config = CreateServerConfig();
                if (config == null)
                {
                    return;
                }

                using (var saveFileDialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    var saveResult = saveFileDialog.ShowDialog();
                    if (saveResult == System.Windows.Forms.DialogResult.OK)
                    {
                        _settings.SaveLocation = saveFileDialog.InitialDirectory;
                        _settings.Save();

                        if(string.IsNullOrEmpty(saveFileDialog.SelectedPath))
                        {
                            MessageBox.Show("Save path is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        WriteWireGuardConfig(config, saveFileDialog.SelectedPath, CheckBoxSaveToZip.IsChecked.HasValue ? CheckBoxSaveToZip.IsChecked.Value : false);

                        MessageBox.Show("Successfully generated configs.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save files with exception: {ex.GetType()}\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void ButtonResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            _settings.Reset();
            _settings.Save();
            LoadDefaults();
        }

        private IPNetwork? ParseAddress()
        {
            return IPNetwork.Parse(InputSubnet.Text);
        }

        private WireGuardServer? CreateServerConfig()
        {
            var wgAddress = ParseAddress();
            if (wgAddress == null)
            {
                MessageBox.Show("Failed to parse address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            int subnetSize = GetUsableSubnetSize(wgAddress);
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

            if (clientCount > subnetSize)
            {
                MessageBox.Show("You cannot have more clients than the subnet addresses available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            // Generate private / public keys.
            var serverKeyPair = CurveKeyPair.GeneratePair();
            if (serverKeyPair == null ||
                serverKeyPair.PrivateKey == null ||
                serverKeyPair.PublicKey == null)
            {
                MessageBox.Show($"Failed to generate server key pair.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            string privKey = serverKeyPair.PrivateKey.ToString();
            string pubKey = serverKeyPair.PublicKey.ToString();

            wgServer.PrivateKey = privKey;
            wgServer.PublicKey = pubKey;

            wgServer.Interface = InputInterface.Text;

            var gatewayAddress = IsGatewayLastIP() ? wgAddress.LastUsable : wgAddress.FirstUsable;
            wgServer.Address = $"{gatewayAddress}/{wgAddress.Cidr}";

            int peerId = 1;
            int i = 0;
            foreach (var address in wgAddress.ListIPAddress(FilterEnum.Usable))
            {
                // Skip this address if it's being used by the gateway.
                if(address.ToString() == gatewayAddress.ToString())
                {
                    continue;
                }

                // Only generate x amount of clients instead of the whole available subnet.
                if(i >= clientCount)
                {
                    break;
                }

                var wgPeer = new WireGuardPeer();

                var peerAddress = address.ToString();

                wgPeer.Id = peerId;
                wgPeer.AllowedIPS = $"{peerAddress}/32";

                // Generate client private / public keys.
                var clientKeyPair = CurveKeyPair.GeneratePair();
                if (clientKeyPair == null ||
                    clientKeyPair.PrivateKey == null ||
                    clientKeyPair.PublicKey == null)
                {
                    MessageBox.Show($"Failed to generate client key pair for client {peerId}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                wgPeer.PublicKey = clientKeyPair.PublicKey.ToString();

                wgServer.PrivateKey = privKey;
                wgServer.PublicKey = pubKey;

                var wgClient = new WireGuardClient();
                wgClient.Address = peerAddress;
                wgClient.Port = wgServer.Port;
                wgClient.AllowedIPS = InputIPs.Text;
                wgClient.PublicKey = wgServer.PublicKey;
                wgClient.PrivateKey = clientKeyPair.PrivateKey.ToString();
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

                i++;
                peerId += 1;
            }
            foreach (var peer in wgServer.Peers)
            {
                wgServer.Commands.Add(ReplaceMacros(wgServer, InputCommand.Text, peer.Id));
            }

            return wgServer;
        }

        private void WriteWireGuardConfig(WireGuardServer wgServer, string path, bool zip = false)
        {
            var sbServer = new StringBuilder();
            var sbClient = new StringBuilder();

            var zipItems = new List<string>();

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
                sbClient.AppendLine($"PublicKey = {wgServer.PublicKey}");

                if (!string.IsNullOrEmpty(peer.Config.PresharedKey))
                    sbClient.AppendLine($"PresharedKey = {peer.Config.PresharedKey}");

                sbClient.AppendLine($"AllowedIPs = {peer.Config.AllowedIPS}");

                if (!string.IsNullOrEmpty(peer.Config.Endpoint))
                    sbClient.AppendLine($"Endpoint = {peer.Config.Endpoint}");

                if (zip)
                {
                    zipItems.Add(Path.Combine(path, $"client{i}.conf"));
                }
                File.WriteAllText(Path.Combine(path, $"client{i}.conf"), sbClient.ToString());
                sbClient.Clear();

                i = i + 1;
            }

            if(zip)
            {
                zipItems.Add(Path.Combine(path, "server.conf"));
            }
            File.WriteAllText(Path.Combine(path, "server.conf"), sbServer.ToString());

            var sbCommands = new StringBuilder();

            if (!string.IsNullOrEmpty(InputPrefix.Text))
            {
                sbCommands.AppendLine(ReplaceMacros(wgServer, InputPrefix.Text));
            }

            foreach (var command in wgServer.Commands)
            {
                sbCommands.AppendLine(ReplaceMacros(wgServer, command));
            }

            if (!string.IsNullOrEmpty(InputPostfix.Text))
            {
                sbCommands.AppendLine(ReplaceMacros(wgServer, InputPostfix.Text));
            }

            if (zip)
            {
                zipItems.Add(Path.Combine(path, "commands.rsc"));
            }

            // Write all commands to file and trim any leading and trailing whitespace (ROS doesn't like trailing blank lines).
            File.WriteAllText(Path.Combine(path, "commands.rsc"), sbCommands.ToString().Trim());

            if(zip)
            {
                // Generate a temporary path to place the configs into, using Base64 with special character stripped.
                string name = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).Replace("/", "").Replace("=", "").Replace("+", "");
                string tempPath = Path.Combine(path, name);
                Directory.CreateDirectory(tempPath);

                // Move all the files into the temporary path.
                foreach(var zipItem in zipItems)
                {
                    File.Move(zipItem, Path.Combine(tempPath, Path.GetFileName(zipItem)));
                }

                // Zip the path and delete the temporary path.
                ZipFile.CreateFromDirectory(tempPath, Path.Combine(path, $"wgc-zipped-{name}.zip"));
                Directory.Delete(tempPath, true);
            }
        }

        private void UpdateMaxClients()
        {
            var wgAddress = ParseAddress();
            if (wgAddress == null)
            {
                LabelNoClients.Content = $"No. Clients (No CIDR Detected)";
                return;
            }

            int subnetSize = GetUsableSubnetSize(wgAddress);
            LabelNoClients.Content = $"No. Clients (Max: {subnetSize})";
        }

        private void InputSubnet_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateMaxClients();
        }

        private string ReplaceMacros(WireGuardServer wgServer, string content, int? clientId = null)
        {
            content = content.Replace("{interface}", wgServer.Interface);

            content = content.Replace("{server-address}", wgServer.Address);
            content = content.Replace("{server-private}", wgServer.PrivateKey);
            content = content.Replace("{server-port}", wgServer.Port.ToString());

            if(clientId != null && clientId.HasValue) 
            {
                var peer = wgServer.Peers.FirstOrDefault(peer => peer.Id == clientId.Value);

                if (peer != null)
                {
                    content = content.Replace("{client-address}", peer.AllowedIPS);
                    content = content.Replace("{client-id}", peer.Id.ToString());
                    content = content.Replace("{client-preshared}", peer.PresharedKey);
                    content = content.Replace("{client-public}", peer.PublicKey);
                }
            }

            return content;
        }

        private bool IsGatewayLastIP()
        {
            return (CheckBoxAssignLastIP.IsChecked.HasValue && CheckBoxAssignLastIP.IsChecked.Value);
        }

        private int GetUsableSubnetSize(IPNetwork? wgAddress)
        {
            if (wgAddress == null)
            {
                return 0;
            }

            int subnetSize = (int)wgAddress.Usable;

            if (IsGatewayLastIP())
            {
                // Subtract the gateway address if assigning as last available.
                subnetSize -= 1;
            }

            return subnetSize;
        }

        private void CheckBoxAssignLastIP_Checked(object sender, RoutedEventArgs e)
        {
            UpdateMaxClients();
        }

        private void CheckBoxAssignLastIP_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateMaxClients();
        }

        private void ButtonTools_Click(object sender, RoutedEventArgs e)
        {
            new ToolsWindow().Show();
        }
    }
}
