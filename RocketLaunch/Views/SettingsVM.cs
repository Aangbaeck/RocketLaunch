using System;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
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
            ApplyBase(S.Settings.DarkTheme);
        }

        public IndexingService IndexService { get; set; }
        public SettingsService S { get; set; }
        public ICommand ToggleThemeCmd { get; } = new RelayCommand<bool>(o => ApplyBase((bool)o));

        public RelayCommand DoubleClickOnItemCmd => new RelayCommand(() => { });

        public RelayCommand<FolderSearch> RemoveSelectedItemFromListCmd => new RelayCommand<FolderSearch>((folder) =>
        {
            S.Settings.SearchDirectories.Remove(folder);
        });

        public RelayCommand RefreshIndexingCmd => new RelayCommand(() => { IndexService.RunIndexing(); });

        public RelayCommand ResetFolderToDefaultCmd => new RelayCommand(() => { S.ResetFolderPaths(); });

        public RelayCommand RemoveAllIndexingCmd => new RelayCommand(() => { IndexService.CleanIndexes(); });

        public RelayCommand ReturnToSearchViewCmd => new RelayCommand(() =>
        {
            Messenger.Default.Send<bool>(true, MessengerID.ReturnToSearchWindow);
        });

        public RelayCommand AddFolderCmd => new RelayCommand(() =>
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog {ShowNewFolderButton = true};

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.SelectedPath;
                S.Settings.SearchDirectories.Add(new FolderSearch() { Path = path });
            }
        });

        public Version AssemblyVersion
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        public int SelectedViewIndex
        {
            get { return _selectedViewIndex; }
            set
            {
                if (value != -1)
                    _selectedViewIndex = value;
                RaisePropertyChanged();
            }
        }

        private static void ApplyBase(bool isDark)
        {
            ModifyTheme(theme => theme.SetBaseTheme(isDark ? Theme.Dark : Theme.Light));
        }

        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            modificationAction?.Invoke(theme);
            paletteHelper.SetTheme(theme);
        }
    }
}