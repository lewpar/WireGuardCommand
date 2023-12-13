using System.Windows;
using WireGuardCommand.ViewModels;

namespace WireGuardCommand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(RootViewModel rootViewModel)
        {
            this.DataContext = rootViewModel;

            InitializeComponent();
        }
    }
}
