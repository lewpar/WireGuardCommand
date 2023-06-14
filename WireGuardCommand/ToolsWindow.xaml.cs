using Elliptic;
using System;
using System.Linq;
using System.Windows;

namespace WireGuardCommand
{
    /// <summary>
    /// Interaction logic for ToolsWindow.xaml
    /// </summary>
    public partial class ToolsWindow : Window
    {
        public ToolsWindow()
        {
            InitializeComponent();
        }

        private void CurveCompare_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(TextBoxPublicKey.Text))
            {
                MessageBox.Show("You need to supply a public key to compare against.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TextBoxPrivateKey.Text))
            {
                MessageBox.Show("You need to supply a public key to compare against.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                byte[] publicKey = Convert.FromBase64String(TextBoxPublicKey.Text);
                byte[] privateKey = Convert.FromBase64String(TextBoxPrivateKey.Text);

                byte[] curvePublicKey = Curve25519.GetPublicKey(privateKey);

                bool result = curvePublicKey.SequenceEqual(publicKey);

                if(result)
                {
                    MessageBox.Show($"The public and private key is a valid pair.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"The public and private key is not a valid pair.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to compare keys with error: {ex.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
