using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog;

namespace RocketLaunch.Model
{
    public static class RunItemFactory
    {
        public static void Start(RunItem item, bool asAdmin, bool openContainingFolder)
        {
            try
            {
                switch (item.Type)
                {
                    case ItemType.File:
                        if (asAdmin)
                        {
                            ExecuteAsAdmin(item.URI);
                        }
                        else if (openContainingFolder)
                        {
                            string args = string.Format("/e, /select, \"{0}\"", item.URI);
                            ProcessStartInfo info = new ProcessStartInfo();
                            info.FileName = "explorer";
                            info.Arguments = args;
                            Process.Start(info);
                        }
                        else  //The normal start
                        {
                            Process.Start(item.URI);
                        }
                        break;
                    case ItemType.Directory:
                        Process.Start(item.URI);
                        break;
                    case ItemType.RunDialog:
                        Task.Run(() => { RunFileDlg(IntPtr.Zero, IntPtr.Zero, null, null, null, 0); });  //Fire away this. We don't need to hold on to it...
                        break;
                    case ItemType.ControlPanelSetting:
                        Process.Start(item.Command);
                        break;
                    case ItemType.TurnOffComputer:
                        Process.Start("shutdown", "/s /t 0");
                        break;
                    case ItemType.RestartComputer:
                        Process.Start("shutdown", "/r /t 0"); // the argument /r is to restart the computer
                        break;
                    case ItemType.LogOffComputer:
                        ExitWindowsEx(0, 0);
                        break;
                    case ItemType.LockComputer:
                        LockWorkStation();
                        break;
                    case ItemType.Hibernate:
                        SetSuspendState(true, true, true);
                        break;
                    case ItemType.Sleep:
                        SetSuspendState(false, true, true);
                        break;

                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not execute this command");
            }
        }
        public static void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            if (Path.GetExtension(fileName) == ".exe")
                proc.StartInfo.Verb = "runas";  //This forces it to use admin rights
            proc.Start();
        }
        public static RunItem Setting(string name, List<string> keyWords, string description, string command, string customIcon)
        {
            return new RunItem() { Name = name, KeyWords = keyWords, URI = description, Type = ItemType.ControlPanelSetting, Command = command, CustomIconName = customIcon };
        }
        public static RunItem RunDialog()
        {
            return new RunItem() { Name = "Run", URI = "Run file dialog Windows", Type = ItemType.RunDialog, CustomIconName = "run_command.png" };
        }
        public static RunItem TurnOffComputer()
        {
            return new RunItem() { Name = "Turn off", URI = "Turn off computer", Type = ItemType.TurnOffComputer, CustomIconName = "turnoff.png" };
        }
        public static RunItem RestartComputer()
        {
            return new RunItem() { Name = "Restart", URI = "Restart computer", Type = ItemType.RestartComputer, CustomIconName = "restart.png" };
        }
        public static RunItem LogOffWindows()
        {
            return new RunItem() { Name = "Log Off", URI = "Log out of windows", Type = ItemType.LogOffComputer, CustomIconName = "logoff.png" };
        }
        public static RunItem LockWindows()
        {
            return new RunItem() { Name = "Lock computer", URI = "Lock this computer", Type = ItemType.LockComputer, CustomIconName = "lockcomputer.png" };
        }
        public static RunItem HibernateWindows()
        {
            return new RunItem() { Name = "Hibernate computer", URI = "Hibernate this computer", Type = ItemType.Hibernate, CustomIconName = "hibernate.png" };
        }
        public static RunItem SleepWindows()
        {
            return new RunItem() { Name = "Sleep ZZzzz", URI = "Make computer sleep", Type = ItemType.Sleep, CustomIconName = "sleep.png" };
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();
        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


        public static List<RunItem> Settings { get; } = new List<RunItem>()
        {
            RunItemFactory.Setting("Settings home page", new List<string>(){"Home"}, "System Settings - Home","ms-settings:","settings3.png"),

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

            RunItemFactory.Setting("Bluetooth & other devices", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:bluetooth","bluetooth.png"),
            RunItemFactory.Setting("Printers & scanners", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:printers","printers.png"),
            RunItemFactory.Setting("Mouse", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:mousetouchpad","mouse.png"),
            RunItemFactory.Setting("Touchpad", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:devices-touchpad","touchpad.png"),
            RunItemFactory.Setting("Typing", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:typing","typing.png"),
            RunItemFactory.Setting("Pen & Windows Ink", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:pen","pen.png"),
            RunItemFactory.Setting("AutoPlay", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:autoplay","autoplay.png"),
            RunItemFactory.Setting("USB", new List<string>(){"Devices"}, "System Settings - Devices","ms-settings:usb","usb.png"),

            RunItemFactory.Setting("Status", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-status","network-status.png"),
            RunItemFactory.Setting("Cellular & SIM", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-cellular","network-cellular.png"),
            RunItemFactory.Setting("Wi-Fi", new List<string>(){"Network & Internet", "Wifi"}, "System Settings - Network & Internet","ms-settings:network-wifi","network-wifi.png"),
            RunItemFactory.Setting("Ethernet", new List<string>(){"Network & Internet", "Wifi"}, "System Settings - Network & Internet","ms-settings:network-ethernet","network-ethernet.png"),
            RunItemFactory.Setting("Dial-up", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-dialup","network-dialup.png"),
            RunItemFactory.Setting("VPN", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-vpn","vpn.png"),
            RunItemFactory.Setting("Airplane mode", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-airplanemode","network-airplanemode.png"),
            RunItemFactory.Setting("Mobile hotspot", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-mobilehotspot","network-mobilehotspot.png"),
            RunItemFactory.Setting("Data usage", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:datausage","datausage.png"),
            RunItemFactory.Setting("Proxy", new List<string>(){"Network & Internet"}, "System Settings - Network & Internet","ms-settings:network-proxy","network-proxy.png"),

            RunItemFactory.Setting("Background", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:personalization-background","personalization-background.png"),
            RunItemFactory.Setting("Colors", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:colors","colors.png"),
            RunItemFactory.Setting("Lock screen", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:lockscreen","lockscreen.png"),
            RunItemFactory.Setting("Themes", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:themes","themes.png"),
            RunItemFactory.Setting("Start", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:personalization-start","personalization-start.png"),
            RunItemFactory.Setting("Taskbar", new List<string>(){"Personalization"}, "System Settings - Personalization","ms-settings:taskbar","taskbar.png"),

            RunItemFactory.Setting("Apps & features", new List<string>(){"Apps"}, "System Settings - Apps","ms-settings:appsfeatures","appsfeatures.png"),
            RunItemFactory.Setting("Manage optional features", new List<string>(){"Apps"}, "System Settings - Apps","ms-settings:optionalfeatures","optionalfeatures.png"),
            RunItemFactory.Setting("Default apps", new List<string>(){"Apps"}, "System Settings - Apps","ms-settings:defaultapps","defaultapps.png"),
            RunItemFactory.Setting("Offline maps", new List<string>(){"Apps"}, "System Settings - Apps","ms-settings:maps","maps.png"),
            RunItemFactory.Setting("Apps for websites", new List<string>(){"Apps"}, "System Settings - Apps","ms-settings:appsforwebsites","appsforwebsites.png"),

            RunItemFactory.Setting("Your info", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:yourinfo","yourinfo.png"),
            RunItemFactory.Setting("Email & app accounts", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:emailandaccounts","emailandaccounts.png"),
            RunItemFactory.Setting("Sign-in options", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:signinoptions","signinoptions.png"),
            RunItemFactory.Setting("Access work or school", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:workplace","workplace.png"),
            RunItemFactory.Setting("Family & other people", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:otherusers","otherusers.png"),
            RunItemFactory.Setting("Sync your settings", new List<string>(){"Accounts"}, "System Settings - Accounts","ms-settings:sync","sync.png"),

            RunItemFactory.Setting("Date & time", new List<string>(), "System Settings - Time","ms-settings:dateandtime","dateandtime.png"),
            RunItemFactory.Setting("Region & language", new List<string>(), "System Settings - Language","ms-settings:regionlanguage","regionlanguage.png"),
            RunItemFactory.Setting("Speech", new List<string>(){"Language"}, "System Settings - Language","ms-settings:speech","speech.png"),

            RunItemFactory.Setting("Game bar", new List<string>(){"Gaming"}, "System Settings - Gaming","ms-settings:gaming-gamebar","gaming-gamebar.png"),
            RunItemFactory.Setting("Game DVR", new List<string>(){"Gaming"}, "System Settings - Gaming","ms-settings:gaming-gamedvr","gaming-gamedvr.png"),
            RunItemFactory.Setting("Broadcasting", new List<string>(){"Gaming"}, "System Settings - Gaming","ms-settings:gaming-broadcasting","gaming-broadcasting.png"),
            RunItemFactory.Setting("Game Mode", new List<string>(){"Gaming"}, "System Settings - Gaming","ms-settings:gaming-gamemode","gaming-gamemode.png"),

            RunItemFactory.Setting("Narrator", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-narrator","easeofaccess-narrator.png"),
            RunItemFactory.Setting("Magnifier", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-magnifier","easeofaccess-magnifier.png"),
            RunItemFactory.Setting("High contrast", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-highcontrast","easeofaccess-highcontrast.png"),
            RunItemFactory.Setting("Closed captions", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-closedcaptioning","easeofaccess-closedcaptioning.png"),
            RunItemFactory.Setting("Keyboard", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-keyboard","easeofaccess-keyboard.png"),
            RunItemFactory.Setting("Mouse", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-mouse","easeofaccess-mouse.png"),
            RunItemFactory.Setting("Other options", new List<string>(){"Ease of Access"}, "System Settings - Ease of Access","ms-settings:easeofaccess-otheroptions","easeofaccess-otheroptions.png"),


            RunItemFactory.Setting("General Privacy", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy","privacy.png"),
            RunItemFactory.Setting("Location", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-location","privacy-location.png"),
            RunItemFactory.Setting("Camera", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-webcam","privacy-webcam.png"),
            RunItemFactory.Setting("Microphone", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-microphone","privacy-microphone.png"),
            RunItemFactory.Setting("Notifications", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-notifications","privacy-notifications.png"),
            RunItemFactory.Setting("Speech, inking, & typing", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-speechtyping","privacy-speechtyping.png"),
            RunItemFactory.Setting("Account info", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-accountinfo","privacy-accountinfo.png"),
            RunItemFactory.Setting("Contacts", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-contacts","privacy-contacts.png"),
            RunItemFactory.Setting("Calendar", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-calendar","privacy-calendar.png"),
            RunItemFactory.Setting("Call history", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-callhistory","privacy-callhistory.png"),
            RunItemFactory.Setting("Email", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-email","privacy-email.png"),
            RunItemFactory.Setting("Tasks", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-tasks","privacy-tasks.png"),
            RunItemFactory.Setting("Messaging", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-messaging","privacy-messaging.png"),
            RunItemFactory.Setting("Radios", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-radios","privacy-radios.png"),
            RunItemFactory.Setting("Other devices", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-customdevices","privacy-customdevices.png"),
            RunItemFactory.Setting("Feedback & diagnostics", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-feedback","privacy-feedback.png"),
            RunItemFactory.Setting("Background apps", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-backgroundapps","privacy-backgroundapps.png"),
            RunItemFactory.Setting("App diagnostics", new List<string>(){"Privacy"}, "System Settings - Privacy","ms-settings:privacy-appdiagnostics","privacy-appdiagnostics.png"),

            RunItemFactory.Setting("Windows Update", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsupdate","windowsupdate.png"),
            RunItemFactory.Setting("Check for updates", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsupdate-action","windowsupdate-action.png"),
            RunItemFactory.Setting("Windows update history", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsupdate-history","windowsupdate-history.png"),
            RunItemFactory.Setting("Restart options", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsupdate-restartoptions","windowsupdate-restartoptions.png"),
            RunItemFactory.Setting("Advanced options", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsupdate-options","windowsupdate-options.png"),
            RunItemFactory.Setting("Windows Defender", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsdefender","windowsdefender.png"),
            RunItemFactory.Setting("Backup", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:backup","backup.png"),
            RunItemFactory.Setting("Troubleshoot", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:troubleshoot","troubleshoot.png"),
            RunItemFactory.Setting("Recovery", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:recovery","recovery.png"),
            RunItemFactory.Setting("Activation", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:activation","activation.png"),
            RunItemFactory.Setting("Find My Device", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:findmydevice","findmydevice.png"),
            RunItemFactory.Setting("For developers", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:developers","developers.png"),
            RunItemFactory.Setting("Windows Insider Program", new List<string>(){"Update & security"}, "System Settings - Update & security","ms-settings:windowsinsider","windowsinsider.png"),

            RunItemFactory.Setting("Mixed reality", new List<string>(){"Mixed reality"}, "System Settings - Mixed reality","ms-settings:holographic","holographic.png"),
            RunItemFactory.Setting("Audio and speech", new List<string>(){"Mixed reality"}, "System Settings - Mixed reality","ms-settings:holographic-audio","holographic-audio.png"),

        };

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