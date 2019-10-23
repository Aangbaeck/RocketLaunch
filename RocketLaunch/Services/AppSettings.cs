using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using RocketLaunch.Model;
using Syroot.Windows.IO;

namespace RocketLaunch.Services
{
    public class AppSettings : ViewModelBase
    {
        private bool _autoStart = true;
        private bool _darkTheme = true;
        private bool _debugMode = false;

        private bool _includeWindowsSettings = true;
        private int _reindexingTime = 20;
        private bool _releaseWinKey;
        private string _windowsToOpenAtStart;

        public bool DebugMode
        {
            get { return _debugMode; }
            set
            {
                _debugMode = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoStart
        {
            get { return _autoStart; }
            set
            {
                _autoStart = value;
                RaisePropertyChanged();
            }
        }

        public string WindowsToOpenAtStart
        {
            get { return _windowsToOpenAtStart; }
            set
            {
                _windowsToOpenAtStart = value;
                RaisePropertyChanged();
            }
        }

        public bool IncludeWindowsSettings
        {
            get { return _includeWindowsSettings; }
            set
            {
                _includeWindowsSettings = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// time between reindexing (minutes)
        /// </summary>
        public int ReindexingTime
        {
            get { return _reindexingTime; }
            set
            {
                _reindexingTime = value;
                RaisePropertyChanged();
            }
        }

        public bool DarkTheme
        {
            get { return _darkTheme; }
            set
            {
                _darkTheme = value;
                RaisePropertyChanged();
            }
        }

        public bool ReleaseWinKey
        {
            get { return _releaseWinKey; }
            set
            {
                _releaseWinKey = value;
                RaisePropertyChanged();
            }
        }

        public BindingList<FolderSearch> SearchDirectories { get; set; } = new BindingList<FolderSearch>();

        public static BindingList<FolderSearch> DefaultFolders => new BindingList<FolderSearch>()
        {
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.AdminTools)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures)},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)},
            new FolderSearch() {Path = new KnownFolder(KnownFolderType.Downloads).Path},
            //new FolderSearch() {Path =  new KnownFolder(KnownFolderType.Contacts).Path},
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)},
            //new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal)},
            //new FolderSearch() {Path = "C:\\Program Files\\WindowsApps", SearchPattern =  "*.exe", IncludeFoldersInSearch = false},
        };

        public void ResetSearchDirectoriesToDefaultFolders()
        {
            SearchDirectories.RaiseListChangedEvents = false;
            SearchDirectories.Clear();
            foreach (var defaultFolder in DefaultFolders)
            {
                SearchDirectories.Add(defaultFolder);
            }

            SearchDirectories.RaiseListChangedEvents = true;
            SearchDirectories.ResetBindings();
        }
    }
}