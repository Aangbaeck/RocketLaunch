using System.ComponentModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Ookii.Dialogs.Wpf;
using RocketLaunch.Model;
using RocketLaunch.Services;

namespace RocketLaunch.Views
{
    public class SettingsVM : ViewModelBase
    {
        private int _selectedViewIndex = 0;

        public SettingsVM(SettingsService s, IndexingService index)
        {
            S = s;
            IndexService = index;


        }
        public IndexingService IndexService { get; set; }
        public SettingsService S { get; set; }

        public RelayCommand DoubleClickOnItemCmd => new RelayCommand(() => { });
        public RelayCommand<FolderSearch> RemoveSelectedItemFromListCmd => new RelayCommand<FolderSearch>((folder) =>
        {
            S.Settings.SearchDirectories.Remove(folder);
        });
        public RelayCommand RefreshIndexingCmd => new RelayCommand(() =>
        {
            IndexService.RunIndexing();
        });
        public RelayCommand ResetFolderToDefaultCmd => new RelayCommand(() =>
        {
            S.ResetFolderPaths();
        });
        public RelayCommand RemoveAllIndexingCmd => new RelayCommand(() =>
        {
            IndexService.CleanIndexes();
        });
        public RelayCommand ReturnToSearchViewCmd => new RelayCommand(() => { Messenger.Default.Send<bool>(true, MessengerID.ReturnToSearchWindow); });
        public RelayCommand AddFolderCmd => new RelayCommand(() =>
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            dlg.ShowNewFolderButton = true;

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.SelectedPath;
                S.Settings.SearchDirectories.Add(new FolderSearch() { Path = path, SearchPattern = "*.*", SearchSubFolders = true });
            }

        });

        public int SelectedViewIndex
        {
            get { return _selectedViewIndex; }
            set { _selectedViewIndex = value; RaisePropertyChanged(); }
        }
    }
}