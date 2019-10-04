using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace RocketLaunch.Model
{
    public static class RunItemFactory
    {
        public static void Start(RunItem item)
        {
            switch (item.Type)
            {
                case ItemType.File:
                    System.Diagnostics.Process.Start(item.URI);
                    break;
                case ItemType.RunDialog:
                    Task.Run(() => { RunFileDlg(IntPtr.Zero, IntPtr.Zero, null, null, null, 0); });  //Fire away this. We don't need to hold on to it...
                    break;
                case ItemType.ControlPanelSetting:
                    System.Diagnostics.Process.Start(item.Command);
                    break;

            }
        }

        public static RunItem Setting(string name, List<string> keyWords, string description, string command, string customIcon)
        {
            return new RunItem() { Name = name, KeyWords = keyWords, URI = description, Type = ItemType.ControlPanelSetting, Command = command, CustomIconName = customIcon };
        }
        public static RunItem RunDialog()
        {
            return new RunItem() { Name = "Run", URI = "Run file dialog Windows", Type = ItemType.RunDialog, CustomIconName = "run_command.png" };
        }

        public static List<RunItem> Settings { get; } = new List<RunItem>()
        {
            RunItemFactory.Setting("Settings home page", new List<string>(){"Home"}, "System Settings","ms-settings:","settings3.png"),

            RunItemFactory.Setting("Display", new List<string>(){"System"}, "System Settings","ms-settings:display","display.png"),
            RunItemFactory.Setting("Notifications & actions", new List<string>(){"System"}, "System Settings","ms-settings:notifications","notifications.png"),
            RunItemFactory.Setting("Power & sleep", new List<string>(){"System"}, "System Settings","ms-settings:powersleep","power.png"),
            RunItemFactory.Setting("Battery", new List<string>(){"System"}, "System Settings","ms-settings:batterysaver","battery.png"),
            RunItemFactory.Setting("Storage", new List<string>(){"System"}, "System Settings","ms-settings:storagesense","storage.png"),
            RunItemFactory.Setting("Tablet mode", new List<string>(){"System"}, "System Settings","ms-settings:tabletmode","tablet.png"),
            RunItemFactory.Setting("Multitasking", new List<string>(){"System"}, "System Settings","ms-settings:multitasking","multitasking.png"),
            RunItemFactory.Setting("Projecting to this PC", new List<string>(){"System"}, "System Settings","ms-settings:project","project.png"),
            RunItemFactory.Setting("Shared experiences", new List<string>(){"System"}, "System Settings","ms-settings:crossdevice","mindmap.png"),
            RunItemFactory.Setting("About your PC", new List<string>(){"System"}, "System Settings","ms-settings:about","about.png"),
        };
        //TODO add more https://winaero.com/blog/ms-settings-commands-windows-10/ and add test that opens all of these to test them out

        [DllImport("shell32.dll", EntryPoint = "#61", CharSet = CharSet.Unicode)]
        private static extern int RunFileDlg(
            [In] IntPtr hWnd,
            [In] IntPtr icon,
            [In] string path,
            [In] string title,
            [In] string prompt,
            [In] uint flags);
    }

}