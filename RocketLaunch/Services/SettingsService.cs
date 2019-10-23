using System;
using System.IO;
using System.Windows.Forms;
using AutoMapper;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using IWshRuntimeLibrary;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using Serilog;
using File = System.IO.File;

namespace RocketLaunch.Services
{
    public class SettingsService : ViewModelBase
    {
        private readonly string _startupPath =
            Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + "RocketLaunch.lnk";

        public SettingsService()
        {
            Settings.ResetSearchDirectoriesToDefaultFolders();
            Load();
            AutoStart = Settings
                .AutoStart; //Make sure the settings are applied and there is a link in the startup folder.
            Settings.PropertyChanged += (sender, args) => { Save(); };
            Settings.SearchDirectories.ListChanged += (sender, args) => { Save(); };
        }

        public AppSettings Settings { get; set; } = new AppSettings();

        public bool AutoStart
        {
            get { return Settings.AutoStart; }
            set
            {
                Settings.AutoStart = value;
                File.Delete(_startupPath); //Reset with new file if there is a new path or something
                if (value)
                    CreateShortCutInAutoStart();

                RaisePropertyChanged();
            }
        }

        public bool ReleaseWinKey
        {
            get { return Settings.ReleaseWinKey; }
            set
            {
                Settings.ReleaseWinKey = value;
                var keyboard = ServiceLocator.Current.GetInstance<KeyBoardHandlerService>();
                if (value)
                    keyboard.ReleaseControlOfWinKey();
                else
                    keyboard.TakeControlOfWinKey();
            }
        }

        public void ResetFolderPaths()
        {
            Settings.ResetSearchDirectoriesToDefaultFolders();
        }

        private void CreateShortCutInAutoStart()
        {
            WshShell wshShell = new WshShell();
            // Create the shortcut
            IWshShortcut shortcut = (IWshShortcut) wshShell.CreateShortcut(_startupPath);

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
                if (File.Exists(Common.SettingsPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Common.SettingsPath) ?? "");
                    var json = File.ReadAllText(Common.SettingsPath);
                    AppSettings localAppsettings = JsonConvert.DeserializeObject<AppSettings>(json);

                    var config = new MapperConfiguration(cfg => { cfg.CreateMap<AppSettings, AppSettings>(); });

                    IMapper iMapper = config.CreateMapper();
                    iMapper.Map(localAppsettings, Settings);
                }
                else
                {
                    Log.Debug("First time starting application. Creating save files");
                    Save();
                }
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