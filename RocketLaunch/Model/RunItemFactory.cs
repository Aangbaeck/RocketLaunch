using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RocketLaunch.Helper;
using Serilog;

namespace RocketLaunch.Model
{
    public static class RunItemFactory
    {
        public static List<RunItem> Settings { get; } = new List<RunItem>()
        {
            Setting("Settings home page", new List<string>() {"Home"}, "System Settings - Home", "ms-settings:",
                "settings3.png"),

            Setting("Display", new List<string>() {"System"}, "System Settings", "ms-settings:display", "display.png"),
            Setting("Notifications & actions", new List<string>() {"System"}, "System Settings",
                "ms-settings:notifications", "notifications.png"),
            Setting("Power & sleep", new List<string>() {"System"}, "System Settings", "ms-settings:powersleep",
                "power.png"),
            Setting("Battery", new List<string>() {"System"}, "System Settings", "ms-settings:batterysaver",
                "battery.png"),
            Setting("Storage", new List<string>() {"System"}, "System Settings", "ms-settings:storagesense",
                "storage.png"),
            Setting("Tablet mode", new List<string>() {"System"}, "System Settings", "ms-settings:tabletmode",
                "tablet.png"),
            Setting("Multitasking", new List<string>() {"System"}, "System Settings", "ms-settings:multitasking",
                "multitasking.png"),
            Setting("Projecting to this PC", new List<string>() {"System"}, "System Settings", "ms-settings:project",
                "project.png"),
            Setting("Shared experiences", new List<string>() {"System"}, "System Settings", "ms-settings:crossdevice",
                "mindmap.png"),
            Setting("About your PC", new List<string>() {"System"}, "System Settings", "ms-settings:about",
                "about.png"),

            Setting("Bluetooth & other devices", new List<string>() {"Devices"}, "System Settings - Devices",
                "ms-settings:bluetooth", "bluetooth.png"),
            Setting("Printers & scanners", new List<string>() {"Devices"}, "System Settings - Devices",
                "ms-settings:printers", "printers.png"),
            Setting("Mouse", new List<string>() {"Devices"}, "System Settings - Devices", "ms-settings:mousetouchpad",
                "mouse.png"),
            Setting("Touchpad", new List<string>() {"Devices"}, "System Settings - Devices",
                "ms-settings:devices-touchpad", "touchpad.png"),
            Setting("Typing", new List<string>() {"Devices"}, "System Settings - Devices", "ms-settings:typing",
                "typing.png"),
            Setting("Pen & Windows Ink", new List<string>() {"Devices"}, "System Settings - Devices", "ms-settings:pen",
                "pen.png"),
            Setting("AutoPlay", new List<string>() {"Devices"}, "System Settings - Devices", "ms-settings:autoplay",
                "autoplay.png"),
            Setting("USB", new List<string>() {"Devices"}, "System Settings - Devices", "ms-settings:usb", "usb.png"),

            Setting("Status", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-status", "network-status.png"),
            Setting("Cellular & SIM", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-cellular", "network-cellular.png"),
            Setting("Wi-Fi", new List<string>() {"Network & Internet", "Wifi"}, "System Settings - Network & Internet",
                "ms-settings:network-wifi", "network-wifi.png"),
            Setting("Ethernet", new List<string>() {"Network & Internet", "Wifi"},
                "System Settings - Network & Internet", "ms-settings:network-ethernet", "network-ethernet.png"),
            Setting("Dial-up", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-dialup", "network-dialup.png"),
            Setting("VPN", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-vpn", "vpn.png"),
            Setting("Airplane mode", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-airplanemode", "network-airplanemode.png"),
            Setting("Mobile hotspot", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-mobilehotspot", "network-mobilehotspot.png"),
            Setting("Data usage", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:datausage", "datausage.png"),
            Setting("Proxy", new List<string>() {"Network & Internet"}, "System Settings - Network & Internet",
                "ms-settings:network-proxy", "network-proxy.png"),

            Setting("Background", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:personalization-background", "personalization-background.png"),
            Setting("Colors", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:colors", "colors.png"),
            Setting("Lock screen", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:lockscreen", "lockscreen.png"),
            Setting("Themes", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:themes", "themes.png"),
            Setting("Start", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:personalization-start", "personalization-start.png"),
            Setting("Taskbar", new List<string>() {"Personalization"}, "System Settings - Personalization",
                "ms-settings:taskbar", "taskbar.png"),

            Setting("Apps & features", new List<string>() {"Apps"}, "System Settings - Apps",
                "ms-settings:appsfeatures", "appsfeatures.png"),
            Setting("Manage optional features", new List<string>() {"Apps"}, "System Settings - Apps",
                "ms-settings:optionalfeatures", "optionalfeatures.png"),
            Setting("Default apps", new List<string>() {"Apps"}, "System Settings - Apps", "ms-settings:defaultapps",
                "defaultapps.png"),
            Setting("Offline maps", new List<string>() {"Apps"}, "System Settings - Apps", "ms-settings:maps",
                "maps.png"),
            Setting("Apps for websites", new List<string>() {"Apps"}, "System Settings - Apps",
                "ms-settings:appsforwebsites", "appsforwebsites.png"),

            Setting("Your info", new List<string>() {"Accounts"}, "System Settings - Accounts", "ms-settings:yourinfo",
                "yourinfo.png"),
            Setting("Email & app accounts", new List<string>() {"Accounts"}, "System Settings - Accounts",
                "ms-settings:emailandaccounts", "emailandaccounts.png"),
            Setting("Sign-in options", new List<string>() {"Accounts"}, "System Settings - Accounts",
                "ms-settings:signinoptions", "signinoptions.png"),
            Setting("Access work or school", new List<string>() {"Accounts"}, "System Settings - Accounts",
                "ms-settings:workplace", "workplace.png"),
            Setting("Family & other people", new List<string>() {"Accounts"}, "System Settings - Accounts",
                "ms-settings:otherusers", "otherusers.png"),
            Setting("Sync your settings", new List<string>() {"Accounts"}, "System Settings - Accounts",
                "ms-settings:sync", "sync.png"),

            Setting("Date & time", new List<string>(), "System Settings - Time", "ms-settings:dateandtime",
                "dateandtime.png"),
            Setting("Region & language", new List<string>(), "System Settings - Language", "ms-settings:regionlanguage",
                "regionlanguage.png"),
            Setting("Speech", new List<string>() {"Language"}, "System Settings - Language", "ms-settings:speech",
                "speech.png"),

            Setting("Game bar", new List<string>() {"Gaming"}, "System Settings - Gaming", "ms-settings:gaming-gamebar",
                "gaming-gamebar.png"),
            Setting("Game DVR", new List<string>() {"Gaming"}, "System Settings - Gaming", "ms-settings:gaming-gamedvr",
                "gaming-gamedvr.png"),
            Setting("Broadcasting", new List<string>() {"Gaming"}, "System Settings - Gaming",
                "ms-settings:gaming-broadcasting", "gaming-broadcasting.png"),
            Setting("Game Mode", new List<string>() {"Gaming"}, "System Settings - Gaming",
                "ms-settings:gaming-gamemode", "gaming-gamemode.png"),

            Setting("Narrator", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-narrator", "easeofaccess-narrator.png"),
            Setting("Magnifier", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-magnifier", "easeofaccess-magnifier.png"),
            Setting("High contrast", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-highcontrast", "easeofaccess-highcontrast.png"),
            Setting("Closed captions", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-closedcaptioning", "easeofaccess-closedcaptioning.png"),
            Setting("Keyboard", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-keyboard", "easeofaccess-keyboard.png"),
            Setting("Mouse", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-mouse", "easeofaccess-mouse.png"),
            Setting("Other options", new List<string>() {"Ease of Access"}, "System Settings - Ease of Access",
                "ms-settings:easeofaccess-otheroptions", "easeofaccess-otheroptions.png"),


            Setting("General Privacy", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy", "privacy.png"),
            Setting("Location", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-location", "privacy-location.png"),
            Setting("Camera", new List<string>() {"Privacy"}, "System Settings - Privacy", "ms-settings:privacy-webcam",
                "privacy-webcam.png"),
            Setting("Microphone", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-microphone", "privacy-microphone.png"),
            Setting("Notifications", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-notifications", "privacy-notifications.png"),
            Setting("Speech, inking, & typing", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-speechtyping", "privacy-speechtyping.png"),
            Setting("Account info", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-accountinfo", "privacy-accountinfo.png"),
            Setting("Contacts", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-contacts", "privacy-contacts.png"),
            Setting("Calendar", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-calendar", "privacy-calendar.png"),
            Setting("Call history", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-callhistory", "privacy-callhistory.png"),
            Setting("Email", new List<string>() {"Privacy"}, "System Settings - Privacy", "ms-settings:privacy-email",
                "privacy-email.png"),
            Setting("Tasks", new List<string>() {"Privacy"}, "System Settings - Privacy", "ms-settings:privacy-tasks",
                "privacy-tasks.png"),
            Setting("Messaging", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-messaging", "privacy-messaging.png"),
            Setting("Radios", new List<string>() {"Privacy"}, "System Settings - Privacy", "ms-settings:privacy-radios",
                "privacy-radios.png"),
            Setting("Other devices", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-customdevices", "privacy-customdevices.png"),
            Setting("Feedback & diagnostics", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-feedback", "privacy-feedback.png"),
            Setting("Background apps", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-backgroundapps", "privacy-backgroundapps.png"),
            Setting("App diagnostics", new List<string>() {"Privacy"}, "System Settings - Privacy",
                "ms-settings:privacy-appdiagnostics", "privacy-appdiagnostics.png"),

            Setting("Windows Update", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:windowsupdate", "windowsupdate.png"),
            Setting("Check for updates", new List<string>() {"Update & security"},
                "System Settings - Update & security", "ms-settings:windowsupdate-action", "windowsupdate-action.png"),
            Setting("Windows update history", new List<string>() {"Update & security"},
                "System Settings - Update & security", "ms-settings:windowsupdate-history",
                "windowsupdate-history.png"),
            Setting("Restart options", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:windowsupdate-restartoptions", "windowsupdate-restartoptions.png"),
            Setting("Advanced options", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:windowsupdate-options", "windowsupdate-options.png"),
            Setting("Windows Defender", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:windowsdefender", "windowsdefender.png"),
            Setting("Backup", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:backup", "backup.png"),
            Setting("Troubleshoot", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:troubleshoot", "troubleshoot.png"),
            Setting("Recovery", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:recovery", "recovery.png"),
            Setting("Activation", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:activation", "activation.png"),
            Setting("Find My Device", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:findmydevice", "findmydevice.png"),
            Setting("For developers", new List<string>() {"Update & security"}, "System Settings - Update & security",
                "ms-settings:developers", "developers.png"),
            Setting("Windows Insider Program", new List<string>() {"Update & security"},
                "System Settings - Update & security", "ms-settings:windowsinsider", "windowsinsider.png"),

            Setting("Mixed reality", new List<string>() {"Mixed reality"}, "System Settings - Mixed reality",
                "ms-settings:holographic", "holographic.png"),
            Setting("Audio and speech", new List<string>() {"Mixed reality"}, "System Settings - Mixed reality",
                "ms-settings:holographic-audio", "holographic-audio.png"),
            Setting("Calculator", new List<string>() {"calc.exe"}, "Windows calculator",
                "calc", "calculator.png"),

            //3D Builder  com.microsoft.builder3d:
            //Action Center   ms-actioncenter:
            //Alarms & Clock  ms-clock:
            //Available Networks  ms-availablenetworks:
            //Calculator  calculator:
            //Calendar    outlookcal:
            //Camera  microsoft.windows.camera:
            //Candy Crush Soda Saga   candycrushsodasaga:
            //Connect ms-projection:
            //Cortana ms-cortana:
            //Cortana Connected Services
            //ms-cortana://notebook/?ConnectedServices
            //Cortana Personal Information
            //ms-cortana://settings/ManageBingProfile
            //Device Discovery    ms-settings-connectabledevices:devicediscovery
            //Drawboard PDF   drawboardpdf:
            //Facebook    fb:
            //Feedback Hub    feedback-hub:
            //Get Help    ms-contact-support:
            //Groove Music    mswindowsmusic:
            //Mail    outlookmail:
            //Maps    bingmaps:
            //ms-drive-to:
            //ms-walk-to:
            //Messaging   ms-chat:
            //Microsoft Edge  microsoft-edge:
            //Microsoft News  bingnews:
            //Microsoft Solitaire Collection  xboxliveapp-1297287741:
            //Microsoft Store ms-windows-store:
            //Microsoft Store - Music
            //microsoftmusic:
            //Microsoft Store - Movies & TV
            //microsoftvideo:
            //Microsoft Whiteboard    ms-whiteboard-cmd:
            //Minecraft: Windows 10 Edition   minecraft:
            //Mixed Reality Camera    ms-holocamera:
            //Mixed Reality Portal    ms-holographicfirstrun:
            //Movies & TV mswindowsvideo:
            //OneNote onenote:
            //Paint 3D    ms-paint:
            //People  ms-people:
            //Photos  ms-photos:
            //Project Display ms-settings-displays-topology:projection
            //Settings    ms-settings:
            //Tips    ms-get-started:
            //Twitter twitter:
            //View 3D Preview com.microsoft.3dviewer:
            //Voice Recorder  ms-callrecording:
            //Weather bingweather:
            //Windows Mixed Reality Environments  ms-environment-builder:
            //Windows Parental Controls   ms-wpc:
            //Windows Security    windowsdefender:
            //Xbox    xbox:
            //Xbox - Friends list
            //xbox-friendfinder:
            //Xbox - Profile page
            //xbox-profile:
            //Xbox - Network settings
            //xbox-network:
            //Xbox - Settings
            //xbox-settings:
            //Xbox One SmartGlass smartglass:
        };

        public static void Start(RunItem item, bool asAdmin, bool openContainingFolder)
        {
            try
            {
                //Fire away. We don't need to hold on to it...
                Task.Run(() =>
                {
                    switch (item.Type)
                    {
                        case ItemType.File:
                            if (asAdmin)
                            {
                                WindowHelper.BringProcessToFrontOrStartIt(item.URI, item.Arguments, asAdmin: true);
                            }
                            else if (openContainingFolder)
                            {
                                string args = $"/e, /select, \"{item.URI}\"";
                                ProcessStartInfo info = new ProcessStartInfo {FileName = "explorer", Arguments = args};
                                Process.Start(info);
                            }
                            else //The normal start
                            {
                                WindowHelper.BringProcessToFrontOrStartIt(item.URI, item.Arguments, asAdmin: false);
                            }

                            break;
                        case ItemType.Link:
                            if (asAdmin)
                            {
                                WindowHelper.BringProcessToFrontOrStartIt(item.Command, item.Arguments, asAdmin: true);
                            }
                            else if (openContainingFolder)
                            {
                                string args = $"/e, /select, \"{item.URI}\"";
                                ProcessStartInfo info = new ProcessStartInfo { FileName = "explorer", Arguments = args };
                                Process.Start(info);
                            }
                            else //The normal start
                            {
                                WindowHelper.BringProcessToFrontOrStartIt(item.Command, item.Arguments, asAdmin: false);
                            }
                            break;
                        case ItemType.Directory:
                            Process.Start(item.URI);
                            break;
                        case ItemType.Win10App:
                            StartWin10App(item);
                            break;
                        case ItemType.RunDialog:
                            RunFileDlg(IntPtr.Zero, IntPtr.Zero, null, null, null, 0);
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
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not execute this command");
            }
        }

        private static void StartWin10App(RunItem item)
        {
            try
            {
                using PowerShell powerShellInstance = PowerShell.Create();
                var command =
                    item.Command.Replace(@"\\", @"\").Replace(@"{", @"""{")
                        .Replace(@"}",
                            @"}"""); //This adds "" to the GUID. Otherwise we cant start it from powershell
                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                powerShellInstance.AddScript($@"start shell:appsFolder\{command}");

                // invoke execution on the pipeline (collecting output)
                powerShellInstance.Invoke();
                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Could not run win10 app {item.Name}");
            }
        }

        public static RunItem Setting(string name, List<string> keyWords, string description, string command,
            string customIcon)
        {
            return new RunItem()
            {
                Name = name, KeyWords = keyWords, URI = description, Type = ItemType.ControlPanelSetting,
                Command = command, IconName = customIcon
            };
        }

        public static RunItem RunDialog()
        {
            return new RunItem()
            {
                Name = "Run", URI = "Run file dialog Windows", Type = ItemType.RunDialog, IconName = "run_command.png"
            };
        }

        public static RunItem TurnOffComputer()
        {
            return new RunItem()
            {
                Name = "Turn off", URI = "Turn off computer", Type = ItemType.TurnOffComputer, IconName = "turnoff.png"
            };
        }

        public static RunItem RestartComputer()
        {
            return new RunItem()
                {Name = "Restart", URI = "Restart computer", Type = ItemType.RestartComputer, IconName = "restart.png"};
        }

        public static RunItem LogOffWindows()
        {
            return new RunItem()
                {Name = "Log Off", URI = "Log out of windows", Type = ItemType.LogOffComputer, IconName = "logoff.png"};
        }

        public static RunItem LockWindows()
        {
            return new RunItem()
            {
                Name = "Lock computer", URI = "Lock this computer", Type = ItemType.LockComputer,
                IconName = "lockcomputer.png"
            };
        }

        public static RunItem HibernateWindows()
        {
            return new RunItem()
            {
                Name = "Hibernate computer", URI = "Hibernate this computer", Type = ItemType.Hibernate,
                IconName = "hibernate.png"
            };
        }

        public static RunItem SleepWindows()
        {
            return new RunItem()
                {Name = "Sleep ZZzzz", URI = "Make computer sleep", Type = ItemType.Sleep, IconName = "sleep.png"};
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


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