using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BetterStart.Helper;
using Serilog;

namespace BetterStart.Model
{
    public class SettingsService
    {
        public string WindowsToOpenAtStart { get; set; }


        public void SaveSettings()
        {
            try
            {
                var dir = Path.GetDirectoryName(Common.SettingsPath) ?? "";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
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
