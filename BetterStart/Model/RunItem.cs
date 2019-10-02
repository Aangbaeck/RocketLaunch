using System;
using System.Collections.Generic;
using System.Linq;
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
        [ProtoMember(1)] public string Name { get; set; }
        [ProtoMember(2)] public string Group { get; set; }
        [ProtoMember(3)] public ItemType Type { get; set; }
        [ProtoMember(4)] public string URI { get; set; }
        [ProtoMember(5)] public int RunNrOfTimes { get; set; } = 0;

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

        public bool HasIcon { get; set; }
        
        public ImageSource Icon
        {
            get
            {

                BitmapSource icon = null;
                try
                {
                    
                    if (Type == ItemType.Directory)
                    {
                        var uri = new Uri("pack://application:,,,/Assets/folder.png");
                        return new BitmapImage(uri);
                    }

                    var path = URI;
                    

                    using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                    {
                        if (sysicon != null)
                            icon = Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch (Exception e)
                {
                    Log.Error("This should not happen since we check this before...");
                }

                return icon;
                //}
                //if(Type == ItemType.Directory)

                //else return null;
            }


        }

        public enum ItemType
        {
            File,
            Setting,
            Directory

        }

        public static class CommonControlPanel
        {
            public static List<RunItem> Settings { get; } = new List<RunItem>()
            {
                new RunItem()
                    {Name = "Settings home page", Group = "Home", URI = "ms-settings:", Type = ItemType.Setting}
            };
            //TODO add more https://winaero.com/blog/ms-settings-commands-windows-10/


        }


    }
}
