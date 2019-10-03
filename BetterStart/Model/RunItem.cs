using System;
using System.Collections.Generic;
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
        [ProtoMember(1)] public string Name { get; set; }
        [ProtoMember(2)] public string Group { get; set; }
        [ProtoMember(3)] public ItemType Type { get; set; }
        [ProtoMember(4)] public string CustomIconName { get; set; } //settings or other manually customized icons
        [ProtoMember(5)] public string URI { get; set; }
        [ProtoMember(6)] public int RunNrOfTimes { get; set; } = 0;

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
                        var uri = new Uri("pack://application:,,,/Assets/folder.png");
                        return new BitmapImage(uri);
                    }

                    if (Type == ItemType.Setting)
                    {
                        var uri = new Uri("pack://application:,,,/Assets/"+ CustomIconName);
                        return new BitmapImage(uri);
                    }
                    if (System.IO.File.Exists(URI))
                    {
                        var path = URI;
                        using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(path))
                        {
                            var handle = (int)sysicon.Handle;
                            icon = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            if (handle == 441650415)   //This 441650415 handle means that it uses a standard icon.
                            {
                                ImageSource sysIcon = IconManager.FindIconForFilename(URI, true);  //Fail silently with 'as'. All we can do is try...
                                if (sysIcon != null)
                                    return sysIcon;
                            }
                        }
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
    }

    public static class RunItemFactory
    {
        public static RunItem Setting(string name, string group, string uri, string customIcon)
        {
            return new RunItem() { Name = name, Group = group, URI = uri, Type = ItemType.Setting, CustomIconName = customIcon };
        }
    }
    public enum ItemType
    {
        File,
        Setting,
        Directory,
    }
}
