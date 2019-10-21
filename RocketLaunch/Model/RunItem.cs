using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RocketLaunch.Helper;
using GalaSoft.MvvmLight;

using ProtoBuf;
using Serilog;
using Exception = System.Exception;

namespace RocketLaunch.Model
{
    [ProtoContract]
    public class RunItem
    {
        [ProtoMember(1)] public string Name { get; set; } //What we will actually search for
        [ProtoMember(2)] public List<string> KeyWords { get; set; } //Add groups, similar words etc. This should be searchable as well
        [ProtoMember(3)] public ItemType Type { get; set; }  //The way we take a decision on what to do with this type
        [ProtoMember(4)] public string Command { get; set; } //The actual command to run for settings
        [ProtoMember(5)] public string IconName { get; set; } //manually customized icons
        [ProtoMember(6)] public string IconBackGround { get; set; } = "Transparent"; //manually customized icons
        [ProtoMember(7)] public string URI { get; set; } //The file path or website or specific text that should be written underneath the Name in the UI.
        [ProtoMember(8)] public string Arguments { get; set; } //The arguments to run with the URI
        [ProtoMember(9)] public int RunNrOfTimes { get; set; } = 0;



        public string FileName
        {
            get
            {
                try
                {
                    if (Type == ItemType.File)
                        return System.IO.Path.GetFileName(URI);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Could not convert URI {URI} to filename");

                }

                return URI;
            }
        }

        public string FileNameWithoutExtension
        {
            get
            {
                try
                {
                    if (Type == ItemType.File)
                        return System.IO.Path.GetFileNameWithoutExtension(URI);
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Could not convert URI {URI} to filename without extension");

                }

                return URI;
            }
        }

        public ImageSource Icon
        {
            get
            {
                
                BitmapSource icon = null;
                try
                {

                    if (Type == ItemType.Directory)
                    {
                        var uri = new Uri("pack://application:,,,/Assets/CustomIcons/folder.png");
                        return new BitmapImage(uri);
                    }
                    if (Type == ItemType.ControlPanelSetting || Type == ItemType.RunDialog)
                    {
                        var uri = new Uri("pack://application:,,,/Assets/CustomIcons/" + IconName);
                        return new BitmapImage(uri);
                    }
                    if (Type == ItemType.Win10App)
                    {
                        if (IconName != null)
                        {
                            var uri = new Uri(IconName);
                            return new BitmapImage(uri);
                        }
                    }
                    if (System.IO.File.Exists(URI))
                    {
                        using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(URI))
                        {
                            var handle = (int)sysicon.Handle;
                            icon = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e,"This should not happen since we check this before...");
                }

                return icon;
            }
        }
    }

    public enum ItemType
    {
        File,
        ControlPanelSetting,
        Win10App,
        Directory,
        RunDialog,
        TurnOffComputer,
        RestartComputer,
        LogOffComputer,
        LockComputer,
        Hibernate,
        Sleep
    }
}
