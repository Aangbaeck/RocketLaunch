using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using Serilog;

namespace RocketLaunch.Model
{
    public class SettingsService: ViewModelBase
    {
        public SettingsService()
        {
            TinyMapper.Bind<SettingsService, SettingsService>();
            this.PropertyChanged += (sender, args) => { Save(); };
        }
        public string WindowsToOpenAtStart { get; set; }
        //public string FolderIconPath => (new Uri("/Assets/folder.ico", UriKind.Relative)).AbsolutePath;
        public int ReindexingTime { get; set; } = 1000000;
        public DateTime LastIndexed { get; set; } = DateTime.MinValue;
        
        public List<(string path, string pattern, bool subFolders)> SearchDirectories { get; set; } = new List<(string, string, bool)> {
            (Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.*", true),
            (Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "*.*", true),
            
            //(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "*.*", true),
            //("C:/Program Files/", "*.exe", true),

        };

        public void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Common.SettingsPath) ?? "");
                var json = File.ReadAllText(Common.SettingsPath);
                var ss = JsonConvert.DeserializeObject<SettingsService>(json);
                TinyMapper.Map(ss,this);  //writing the settings back to itself without destroying the handle
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
