using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using Serilog;
using Syroot.Windows.IO;

namespace RocketLaunch.Model
{
    public class SettingsService : ViewModelBase
    {
        private bool _addWindowsSettings;

        public SettingsService()
        {
            TinyMapper.Bind<SettingsService, SettingsService>();
            this.PropertyChanged += (sender, args) => { Save(); };
        }
        public string WindowsToOpenAtStart { get; set; }

        //public string FolderIconPath => (new Uri("/Assets/folder.ico", UriKind.Relative)).AbsolutePath;
        public bool AddWindowsSettings
        {
            get { return _addWindowsSettings; }
            set { _addWindowsSettings = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// time between reindexing (minutes)
        /// </summary>
        public int ReindexingTime { get; set; } = 20;
        public DateTime LastIndexed { get; set; } = DateTime.MinValue;

        public ObservableCollection<FolderSearch> SearchDirectories { get; set; } = new ObservableCollection<FolderSearch>()
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
            //new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer), SearchPattern =  "*.*", SearchSubFolders = true },
           
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal), SearchPattern =  "*.*", SearchSubFolders = true },
            new FolderSearch() {Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup), SearchPattern =  "*.*", SearchSubFolders = true },
        };

        public void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Common.SettingsPath) ?? "");
                var json = File.ReadAllText(Common.SettingsPath);
                var ss = JsonConvert.DeserializeObject<SettingsService>(json);
                TinyMapper.Map(ss, this);  //writing the settings back to itself without destroying the handle
            }

            catch (Exception e)
            {
                Log.Error("Could not save settings." + e.Message + e.StackTrace);
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Common.SettingsPath) ?? "");
                var results = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Common.SettingsPath, results);
            }

            catch (Exception e)
            {
                Log.Error("Could not save settings." + e.Message + e.StackTrace);
            }
        }
    }


}
