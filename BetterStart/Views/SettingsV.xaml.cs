using System;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using RocketLaunch.Helper;

namespace RocketLaunch.Views
{
    /// <summary>
    /// Description for TheSecondWindowV.
    /// </summary>
    public partial class SecondV : Window
    {
        /// <summary>
        /// Initializes a new instance of the TheSecondWindowV class.
        /// </summary>
        public SecondV()
        {
            InitializeComponent();
        }
        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }




    }
}