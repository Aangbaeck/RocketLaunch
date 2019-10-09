using System;
using System.IO;
using System.Windows.Forms;
using AutoMapper;
using GalaSoft.MvvmLight;
using IWshRuntimeLibrary;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using Serilog;
using File = System.IO.File;

namespace RocketLaunch.Services
{
    public class SettingsService: ViewModelBase
    {
        public AppSettings Settings { get; set; } = new AppSettings();
        public SettingsService()
        {
            Settings.ResetSearchDirectoriesToDefaultFolders();
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

        public void ResetFolderPaths()
        {
            Settings.ResetSearchDirectoriesToDefaultFolders();
        }
        public bool AutoStart
        {
            get { return Settings.AutoStart; }
            set
            {
                Settings.AutoStart = value;
                if (value)
                    CreateShortCutInAutoStart();
                else
                    File.Delete(_startupPath);
                RaisePropertyChanged();
            }
        }

        private readonly string _startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + "RocketLaunch.lnk";
        private void CreateShortCutInAutoStart()
        {
            WshShell wshShell = new WshShell();
            // Create the shortcut
            IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(_startupPath);

            shortcut.TargetPath = Application.ExecutablePath;
            shortcut.WorkingDirectory = Application.StartupPath;
            shortcut.Description = "Launch RocketLaunch Application";
            // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
            shortcut.Save();
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
