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
        [ProtoMember(5)] public string CustomIconName { get; set; } //manually customized icons
        [ProtoMember(6)] public string URI { get; set; } //The file path or website or specific text that should be written underneath the Name in the UI.
        [ProtoMember(7)] public int RunNrOfTimes { get; set; } = 0;



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
                        var uri = new Uri("pack://application:,,,/Assets/CustomIcons/" + CustomIconName);
                        return new BitmapImage(uri);
                    }

                    var sw = new Stopwatch();
                    sw.Start();
                    var t = sw.ElapsedTicks;
                    if (System.IO.File.Exists(URI))
                    {
                        
                        var path = URI;
                        using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                        {
                            var handle = (int)sysicon.Handle;
                            icon = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            //if (handle == 441650415)   //This 441650415 handle means that it uses a standard icon.
                            //{
                            //    ImageSource sysIcon = IconManager.FindIconForFilename(URI, true);  //Fail silently with 'as'. All we can do is try...
                            //    if (sysIcon != null)
                            //    {
                            //        t = sw.ElapsedTicks;
                            //        return sysIcon;
                            //    }
                            //}
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Error(e,"This should not happen since we check this before...");
                }

                return icon;
                //}
                //if(Type == ItemType.Directory)

                //else return null;
            }


        }
    }

    public enum ItemType
    {
        File,
        ControlPanelSetting,
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
