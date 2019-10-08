using System;
using System.IO;
using AutoMapper;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using Serilog;

namespace RocketLaunch.Services
{
    public class SettingsService
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public SettingsService()
        {
            Load();
            Settings.PropertyChanged += (sender, args) =>
            {
                Save();
            };
            Settings.SearchDirectories.ListChanged += (sender, args) =>
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
                AppSettings localAppsettings = JsonConvert.DeserializeObject<AppSettings>(json);

                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<AppSettings, AppSettings>();
                });

                IMapper iMapper = config.CreateMapper();
                iMapper.Map(localAppsettings,Settings);

            }

            catch (Exception e)
            {
                Log.Error("Could not load settings." + e.Message + e.StackTrace);
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
