using System.Windows;
using System.Windows.Controls;

namespace WireGuardCommand.Resources.UserControls
{
    /// <summary>
    /// Interaction logic for InfoTooltip.xaml
    /// </summary>
    public partial class InfoToolTip : UserControl
    {
        public string Information
        {
            get { return (string)GetValue(InformationProperty); }
            set { SetValue(InformationProperty, value); }
        }

        public static readonly DependencyProperty InformationProperty =
            DependencyProperty.Register("Information", typeof(string), typeof(InfoToolTip), new PropertyMetadata(default(string)));


        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(InfoToolTip), new PropertyMetadata(default(string)));


        public InfoToolTip()
        {
            InitializeComponent();
        }
    }
}
