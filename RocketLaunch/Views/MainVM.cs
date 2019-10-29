using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using RocketLaunch.Services;
using Serilog;

namespace RocketLaunch.Views
{
    public class MainVM : ViewModelBase
    {
        private bool _isFocused;

        private int _renderingTime;


        private int _selectedIndex = -1;
        private int _selectedViewIndex = 0;
        private bool currentlySearching;
        private bool forceNewSearch;

        private string localSearchString;


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
            Messenger.Default.Register<bool>(this, MessengerID.ReturnToSearchWindow, (e) => { SelectedViewIndex = 0; });
            forceNewSearch = true;

            var searchTimer = new System.Timers.Timer(100);
            searchTimer.Start();
            searchTimer.Elapsed += (_, __) =>
            {
                CheckNewSearchString(); //there is no need to check faster than 100 ms.
            };
        }

        

        public RelayCommand OpenSettingsWindowCmd => new RelayCommand(() => { SelectedViewIndex = 1; });
        public RelayCommand OpenAboutWindowCmd => new RelayCommand(() => { SelectedViewIndex = 2; });

        public RelayCommand ToggleDebugMode =>
            new RelayCommand(() => { S.Settings.DebugMode = !S.Settings.DebugMode; });

        public RelayCommand ExecuteFirstListViewItem => new RelayCommand(() => { ExecuteSelectedListViewItem(false); });
        public RelayCommand ExecuteAsAdminCmd => new RelayCommand(() => { ExecuteSelectedListViewItem(true); });
        public RelayCommand ResetCounterCmd => new RelayCommand(ResetCounter);


        public RelayCommand OpenContaningFolderCmd => new RelayCommand(() =>
        {
            ExecuteSelectedListViewItem(openContainingFolder: true);
        });

        public RelayCommand CloseApplicationCmd => new RelayCommand(() => { Application.Current.Shutdown(); });
        public RelayCommand DoubleClickOnItemCmd => new RelayCommand(() => { ExecuteSelectedListViewItem(); });


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
            set
            {
                _selectedViewIndex = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand OpenLogFile => new RelayCommand(() =>
        {
            var directory = Path.GetDirectoryName(Common.LogfilesPath);
            if (directory != null && Directory.Exists(directory))
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
            else
            {
                Log.Error($"Directory {directory} does not exist.");
            }
        });

        public string SearchString { get; set; }

        public int RenderingTime
        {
            get { return _renderingTime; }
            set
            {
                _renderingTime = value;
                RaisePropertyChanged();
            }
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

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged();
            }
        }

        public SettingsService S { get; set; }

        public IndexingService Indexing { get; set; }

        private void ExecuteSelectedListViewItem(bool asAdmin = false, bool openContainingFolder = false)
        {
            try
            {
                var index = 0;
                if (SelectedIndex != -1)
                    index = SelectedIndex;
                else
                {
                }

                Messenger.Default.Send<bool>(true, MessengerID.HideWindow);

                if (SearchSuggestions.Count > 0)
                {
                    RunItemFactory.Start(SearchSuggestions[index], asAdmin, openContainingFolder);
                    if (!openContainingFolder
                    ) //We didn't actually start the application. Just the folder. Ignore the counter
                    {
                        Indexing.AddExecutedItem(SearchSuggestions[index]);
                        Indexing.SaveTries();
                    }

                    forceNewSearch = true;
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
            if (SelectedIndex != -1)
                index = SelectedIndex;
            else
            {
            }

            if (SearchSuggestions.Count > 0)
            {
                Indexing.ResetItemRunCounter(SearchSuggestions[index]);
                Indexing.SaveTries();
                forceNewSearch = true;
            }
        }

        private void CheckNewSearchString()
        {
            if (forceNewSearch || SearchString != localSearchString && !currentlySearching)
            {
                forceNewSearch = false;
                currentlySearching = true;
                localSearchString = SearchString;
                var sw = new Stopwatch();
                sw.Start();
                SelectedIndex = -1;
                List<RunItem> list = Indexing.Search(localSearchString);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchSuggestions.RaiseListChangedEvents = false;
                    SearchSuggestions.Clear();
                    foreach (var item in list)
                    {
                        SearchSuggestions.Add(item);
                    }

                    SearchSuggestions.RaiseListChangedEvents = true;
                    SearchSuggestions.ResetBindings();
                }, DispatcherPriority.Background);

                sw.Stop();
                RenderingTime = (int) sw.ElapsedMilliseconds;
                currentlySearching = false;
            }
        }

        private void HandleWindowDeactivated(bool obj)
        {
            SelectedIndex = -1;
            if (SelectedViewIndex != 0)
                return; //Allow window to stay up
            Messenger.Default.Send<bool>(true, MessengerID.HideWindow);
        }

        
    }
}