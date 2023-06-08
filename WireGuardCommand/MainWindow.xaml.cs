using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void ButtonClearDefaults_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            LoadDefaults();
        }
    }
}
