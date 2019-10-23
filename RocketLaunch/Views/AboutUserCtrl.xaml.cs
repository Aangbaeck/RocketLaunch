using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using Serilog;

namespace RocketLaunch.Views
{
    /// <summary>
    /// Interaction logic for AboutUserCtrl.xaml
    /// </summary>
    public partial class AboutUserCtrl : UserControl
    {
        public AboutUserCtrl()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Could not open adress {e.Uri.AbsoluteUri}");
            }
        }
    }
}