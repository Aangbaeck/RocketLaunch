using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StartMenu2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            using (var kh = new KeyboardHook(KeyboardHook.Parameters.AllowAltTab))
            {
                kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(kh_KeyIntercepted);



                InitializeComponent();
            }
        }

        private static void kh_KeyIntercepted(KeyboardHook.KeyboardHookEventArgs e)
        {
            if (e.KeyName == "LWin")
            {
                System.Windows.MessageBox.Show("Windows key");
            }

        }
    }
}

