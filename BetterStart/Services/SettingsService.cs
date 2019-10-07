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
using RocketLaunch.Services;
using Serilog;
using Syroot.Windows.IO;

namespace RocketLaunch.Model
{
    public class SettingsService
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public SettingsService()
        {
            TinyMapper.Bind<AppSettings, AppSettings>();
            Load();
            Settings.PropertyChanged += (sender, args) => { Save(); };
            Settings.SearchDirectories.CollectionChanged += (sender, args) =>
            {
                Save();
            };
        }
        
        

        public void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Common.SettingsPath) ?? "");
                var json = File.ReadAllText(Common.SettingsPath);
                var ss = JsonConvert.DeserializeObject<AppSettings>(json);
                TinyMapper.Map(ss, Settings);  //writing the settings back to itself without destroying the handle
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
                var results = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(Common.SettingsPath, results);
            }

            catch (Exception e)
            {
                Log.Error("Could not save settings." + e.Message + e.StackTrace);
            }
        }
    }


}
