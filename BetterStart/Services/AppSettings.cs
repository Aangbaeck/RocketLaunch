using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using RocketLaunch.Model;
using Syroot.Windows.IO;

namespace RocketLaunch.Services
{
    public class AppSettings: ViewModelBase
    {
        private bool _debugMode = false;
        public bool DebugMode
        {
            get { return _debugMode; }
            set { _debugMode = value; RaisePropertyChanged(); }
        }

        public bool AutoStart
        {
            get { return _autoStart; }
            set { _autoStart = value; RaisePropertyChanged();}
        }

        public string WindowsToOpenAtStart
        {
            get { return _windowsToOpenAtStart; }
            set { _windowsToOpenAtStart = value; RaisePropertyChanged(); }
        }

        private bool _includeWindowsSettings = true;
        private string _windowsToOpenAtStart;
        private int _reindexingTime = 20;
        private bool _autoStart;

        public bool IncludeWindowsSettings
        {
            get { return _includeWindowsSettings; }
            set { _includeWindowsSettings = value; RaisePropertyChanged(); }
        }
        /// <summary>
        /// time between reindexing (minutes)
        /// </summary>
        public int ReindexingTime
        {
            get { return _reindexingTime; }
            set { _reindexingTime = value; RaisePropertyChanged(); }
        }

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
        public BindingList<FolderSearch> SearchDirectories { get; set; } = new BindingList<FolderSearch>();
        public static BindingList<FolderSearch> DefaultFolders => new BindingList<FolderSearch>()
        {
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), SearchPattern = "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), SearchPattern = "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), SearchPattern = "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.AdminTools), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path =  new KnownFolder(KnownFolderType.Downloads).Path, SearchPattern =  "*.*", SearchSubFolders = true },
            //new FolderSearch() {Path =  new KnownFolder(KnownFolderType.Contacts).Path, SearchPattern =  "*.*", SearchSubFolders = true },
            //new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup), SearchPattern =  "*.*", SearchSubFolders = true },
        };
    }
}
