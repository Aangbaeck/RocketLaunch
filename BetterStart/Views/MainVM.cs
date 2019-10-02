using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace RocketLaunch.Views
{
    public class MainVM : ViewModelBase
    {


        private bool _isFocused;

        public RelayCommand OpenNewWindowCmd => new RelayCommand(() => { Messenger.Default.Send(typeof(SecondV), MessengerID.MainWindowV); });
        public RelayCommand ToggleDebugMode => new RelayCommand(() => { DebugMode = !DebugMode; });
        public RelayCommand ExecuteFirstListViewItem => new RelayCommand(ExecuteSelectedListViewItem);
        public RelayCommand<RunItem> DoubleClickOnItemCmd  => new RelayCommand<RunItem>(ExecuteThisListViewItem);

        private void ExecuteThisListViewItem(RunItem run)
        {
            Messenger.Default.Send<bool>(true, MessengerID.HideWindow);
            
            
                System.Diagnostics.Process.Start(run.URI);
                SearchSuggestions[SelectedIndex].RunNrOfTimes++;
                Indexing.AddExecutedItem(SearchSuggestions[SelectedIndex]);
                Indexing.SavePrioTrie();
            
        }

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
        public RelayCommand AnyKeyPressedCmd => new RelayCommand(AnyKeyPressed);

        private void AnyKeyPressed()
        {
        }

        public RelayCommand TabOrArrow => new RelayCommand(ChangeSelectedIndex);

        private void ChangeSelectedIndex()
        {
        }

        private void ExecuteSelectedListViewItem()
        {

            try
            {
                Messenger.Default.Send<bool>(true, MessengerID.HideWindow);
                var index = 0;
                if (SelectedIndex > 0)
                    index = SelectedIndex;
                if (SearchSuggestions.Count > 0)
                {
                    System.Diagnostics.Process.Start(SearchSuggestions[index].URI);
                    SearchSuggestions[index].RunNrOfTimes++;
                    Indexing.AddExecutedItem(SearchSuggestions[index]);
                    Indexing.SavePrioTrie();
                }
                //// Prepare the process to run
                //ProcessStartInfo start = new ProcessStartInfo();
                //// Enter in the command line arguments, everything you would enter after the executable name itself
                ////start.Arguments = arguments;
                //// Enter the executable to run, including the complete path
                //start.FileName = SearchSuggestions[SelectedIndex].URI;
                //// Do you want to show a console window?
                ////start.WindowStyle = ProcessWindowStyle.Hidden;
                ////start.CreateNoWindow = true;
                //int exitCode;


                //// Run the external process & wait for it to finish
                //using (Process proc = Process.Start(start))
                //{
                //    //proc.WaitForExit();

                //    // Retrieve the app's exit code
                //    //exitCode = proc.ExitCode;
                //}
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not execute application");

            }

        }

        public bool DebugMode
        {
            get { return _debugMode; }
            set { _debugMode = value; RaisePropertyChanged(); }
        }

        public RelayCommand OpenLogFile => new RelayCommand(() =>
        {
            var directory = Path.GetDirectoryName(Common.LogfilesPath); /*+ "Logfile.log";*/
            if (directory != null)
            {
                Directory.CreateDirectory(directory);

                var files = Directory.GetFiles(directory, "*.log");
                if (files.Length > 0)
                {
                    var filePath = files[files.Length - 1];
                    if (File.Exists(filePath))
                    {
                        System.Diagnostics.Process.Start(filePath);
                    }
                }
            }
        });


        //CancellationTokenSource source = new CancellationTokenSource();
        //CancellationToken token;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                
                List<RunItem> list = Indexing.Search(value);
                SearchSuggestions.Clear();
                SearchSuggestions.AddRange(list);
                SelectedIndex = 0;
                //var sw = new Stopwatch();
                //sw.Start();

                //source.Cancel();  //Close old searches
                //source = new CancellationTokenSource();
                //token = source.Token;
                //Task.Run(() =>
                //{

                //Dispatcher.CurrentDispatcher.Invoke(() => { SearchSuggestions.Clear(); }, DispatcherPriority.DataBind);
                //List<RunItem> list = Indexing.Search(value);
                //Dispatcher.CurrentDispatcher.Invoke(() => { SearchSuggestions.AddRange(list); }, DispatcherPriority.DataBind);
                //}, token);


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

        public WpfObservableRangeCollection<RunItem> SearchSuggestions { get; set; } = new WpfObservableRangeCollection<RunItem>();

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value != -1)
                    _selectedIndex = value;
                RaisePropertyChanged();

            }
        }

        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private bool _debugMode = true;

        private string _searchString;
        private int _selectedIndex = 0;

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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class. IOC
        /// </summary>
        public MainVM(IndexingService index)
        {
            Indexing = index;
            //ApplyBase(true);
            IsFocused = false;
            IsFocused = true;
        }

        public IndexingService Indexing { get; set; }
    }
}