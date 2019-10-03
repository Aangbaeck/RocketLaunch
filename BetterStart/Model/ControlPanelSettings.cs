using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLaunch.Model
{
    public static class CommonControlPanel
    {
        public static List<RunItem> Settings { get; } = new List<RunItem>()
        {
            RunItemFactory.Setting("Settings home page","Home","ms-settings:","settings.png")
            
        };
        //TODO add more https://winaero.com/blog/ms-settings-commands-windows-10/


    }
}
