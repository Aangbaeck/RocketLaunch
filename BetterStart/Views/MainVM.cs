using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using BetterStart.Helper;
using BetterStart.Indexing;
using BetterStart.Model;
using BetterStart.Services;
using MaterialDesignThemes.Wpf;
using Serilog;

namespace BetterStart.Views
{
    public class MainVM : ViewModelBase
    {


        public readonly DataService D;
        private bool _isFocused;

        public RelayCommand OpenNewWindowCmd => new RelayCommand(() => { Messenger.Default.Send(typeof(SecondV), MessengerID.MainWindowV); });
        public RelayCommand ToggleDebugMode => new RelayCommand(() => { DebugMode = !DebugMode; });

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

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                
                SearchSuggestions.Clear();
                var list = Indexing.Search(value);
                for (var index = 0; index < Math.Min(list.Count, 10); index++)
                {
                    var result = list[index];
                    SearchSuggestions.Add(new ItemInfo() { Path = result.Name });
                }
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

        public ObservableCollection<ItemInfo> SearchSuggestions { get; set; } = new ObservableCollection<ItemInfo>();


        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private bool _debugMode = true;
        
        private string _searchString;

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