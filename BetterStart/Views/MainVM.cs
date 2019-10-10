using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using RocketLaunch.Services;
using MaterialDesignThemes.Wpf;
using Serilog;


namespace RocketLaunch.Views
{
    public class MainVM : ViewModelBase
    {


        private bool _isFocused;

        public RelayCommand OpenSettingsWindowCmd => new RelayCommand(() => { SelectedViewIndex = 1; });
        public RelayCommand OpenAboutWindowCmd => new RelayCommand(() => { SelectedViewIndex = 2; });
        public RelayCommand ToggleDebugMode => new RelayCommand(() => { S.Settings.DebugMode = !S.Settings.DebugMode; });
        public RelayCommand ExecuteFirstListViewItem => new RelayCommand(() => { ExecuteSelectedListViewItem(false); });
        public RelayCommand ExecuteAsAdminCmd => new RelayCommand(() => { ExecuteSelectedListViewItem(true); });
        public RelayCommand ResetCounterCmd => new RelayCommand(ResetCounter );

        

        public RelayCommand OpenContaningFolderCmd => new RelayCommand(() => { ExecuteSelectedListViewItem(openContainingFolder:true); });
        public RelayCommand CloseApplicationCmd => new RelayCommand(() => { System.Windows.Application.Current.Shutdown(); });
        public RelayCommand DoubleClickOnItemCmd => new RelayCommand(() => { ExecuteSelectedListViewItem(false); });
        public RelayCommand ShowRightClickMenuCmd => new RelayCommand(() =>
        {

        });

        public RelayCommand DownKeyPressedCmd => new RelayCommand(() =>
              {
                  if (SelectedIndex < SearchSuggestions.Count - 1)
                      SelectedIndex++;
              });
        public RelayCommand UpKeyPressedCmd => new RelayCommand(() =>
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
        });


        public int SelectedViewIndex
        {
            get { return _selectedViewIndex; }
            set { _selectedViewIndex = value; RaisePropertyChanged(); }
        }

        public RelayCommand OpenLogFile => new RelayCommand(() =>
        {
            var directory = Path.GetDirectoryName(Common.LogfilesPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);

                var files = Directory.GetFiles(directory, "*.log");
                if (files.Length > 0)
                {
                    var filePath = files[files.Length - 1];
                    if (File.Exists(filePath))
                    {
                        Process.Start(filePath);
                    }
                }
            }
        });

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                var sw = new Stopwatch();
                sw.Start();
                _searchString = value;
                SelectedIndex = -1;
                List<RunItem> list = Indexing.Search(value);
                SearchSuggestions.RaiseListChangedEvents = false;
                SearchSuggestions.Clear();
                foreach (var item in list)
                {
                    SearchSuggestions.Add(item);
                }
                SearchSuggestions.RaiseListChangedEvents = true;
                SearchSuggestions.ResetBindings();
                sw.Stop();
                RenderingTime = (int)sw.ElapsedMilliseconds;

            }
        }
        private int _renderingTime;

        public int RenderingTime
        {
            get { return _renderingTime; }
            set { _renderingTime = value; RaisePropertyChanged(); }
        }

        public bool IsFocused
        {
            get { return _isFocused; }
            set
            {
                _isFocused = value;
                RaisePropertyChanged();
            }
        }

        public BindingList<RunItem> SearchSuggestions { get; set; } = new BindingList<RunItem>();
        //public ObservableCollection<RunItem> SearchSuggestions { get; set; } = new ObservableCollection<RunItem>();


        

        private void ExecuteSelectedListViewItem(bool asAdmin = false, bool openContainingFolder = false)
        {
            try
            {
                Messenger.Default.Send<bool>(true, MessengerID.HideWindow);
                var index = 0;
                if (StoredSelectedIndex > 0)
                    index = StoredSelectedIndex;
                if (SearchSuggestions.Count > 0)
                {
                    RunItemFactory.Start(SearchSuggestions[index], asAdmin, openContainingFolder);
                    if (!openContainingFolder) //We didn't actually start the application. Just the folder. Ignore the counter
                    {
                        Indexing.AddExecutedItem(SearchSuggestions[index]);
                        Indexing.SavePrioTrie();
                    }

                    SearchString = _searchString;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not execute application");

            }
        }
        private void ResetCounter()
        {
            var index = 0;
            if (StoredSelectedIndex > 0)
                index = StoredSelectedIndex;
            if (SearchSuggestions.Count > 0)
            {
                Indexing.ResetItemRunCounter(SearchSuggestions[index]);
                Indexing.SavePrioTrie();
                SearchString = _searchString;
            }
        }
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value != -1)
                    StoredSelectedIndex = value;
                _selectedIndex = value;
                RaisePropertyChanged();
            }
        }

        public int StoredSelectedIndex { get; set; }

        


        private string _searchString;
        private int _selectedIndex = -1;
        private int _selectedViewIndex = 0;

        
        /// <summary>
        /// Initializes a new instance of the MainViewModel class. IOC
        /// </summary>
        public MainVM(IndexingService index, SettingsService s)
        {
            Indexing = index;
            //ApplyBase(true);
            IsFocused = false;
            IsFocused = true;
            S = s;
            Messenger.Default.Register<bool>(this, MessengerID.WindowDeactivated, HandleWindowDeactivated);
            Messenger.Default.Register<bool>(this, MessengerID.ReturnToSearchWindow, (e) =>
            {
                SelectedViewIndex = 0;
            });
        }

        public SettingsService S { get; set; }

        private void HandleWindowDeactivated(bool obj)
        {
            SelectedIndex = -1;
            if (SelectedViewIndex != 0)
                return; //Allow window to stay up
            Messenger.Default.Send<bool>(true, MessengerID.HideWindow);

        }

        public IndexingService Indexing { get; set; }
    }
}