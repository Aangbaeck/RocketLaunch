using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
