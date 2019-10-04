using System.ComponentModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ookii.Dialogs.Wpf;
using RocketLaunch.Model;

namespace RocketLaunch.Views
{
    public class SettingsVM : ViewModelBase
    {
        public SettingsVM(SettingsService s)
        {
            S = s;


        }
        public RelayCommand DoubleClickOnItemCmd => new RelayCommand(()=> { });
        public RelayCommand AddFolderCmd => new RelayCommand(() =>
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            dlg.ShowNewFolderButton = true;

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.SelectedPath;
            }

        });
        public SettingsService S { get; set; }

        public int SelectedIndex { get; set; }
}
}