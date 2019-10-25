using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ProtoBuf;
using Serilog;

namespace RocketLaunch.Model
{
    [ProtoContract]
    public class RunItem
    {
        [ProtoMember(1)] public string Name { get; set; } //What we will actually search for

        [ProtoMember(2)]
        public List<string> KeyWords { get; set; } = new List<string>(); //Add groups, similar words etc. This should be searchable as well

        [ProtoMember(3)] public ItemType Type { get; set; } //The way we take a decision on what to do with this type
        [ProtoMember(4)] public string Command { get; set; } //The actual command to run for settings
        [ProtoMember(5)] public string IconName { get; set; } //This is either icon names or the icon URI
        [ProtoMember(6)] public int IconNr { get; set; } = 0; //the number of the icons that are located
        [ProtoMember(7)] public string IconBackGround { get; set; } = "Transparent"; //manually customized icons

        [ProtoMember(8)]
        public string
            URI
        {
            get;
            set;
        } //The file path or website or specific text that should be written underneath the Name in the UI.

        [ProtoMember(9)] public string Arguments { get; set; } //The arguments to run with the URI
        [ProtoMember(10)] public string Tooltip { get; set; } //Something interesting to show about the file. Links contain descriptions that can be interesting to show here.
        [ProtoMember(11)] public int RunNrOfTimes { get; set; } = 0;


        public string FileName
        {
            get
            {
                try
                {
                    if (Type == ItemType.File)
                        return Path.GetFileName(URI);
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
                        return Path.GetFileNameWithoutExtension(URI);
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

                    if (   Type == ItemType.ControlPanelSetting
                        || Type == ItemType.RunDialog
                        || Type == ItemType.Hibernate
                        || Type == ItemType.LockComputer 
                        || Type == ItemType.RestartComputer 
                        || Type == ItemType.LogOffComputer 
                        || Type == ItemType.TurnOffComputer 
                        || Type == ItemType.Sleep)
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

                    if (Type == ItemType.Link)
                    {
                        if (IconName != null)
                        {
                            try
                            {
                                if (File.Exists(IconName))
                                {
                                    using System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(URI);

                                    if (sysicon != null)
                                    {
                                        var handle = (int)sysicon.Handle;
                                        return icon = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty,
                                            BitmapSizeOptions.FromEmptyOptions());
                                    }
                                    Stream iconStream = new FileStream(IconName, FileMode.Open, FileAccess.Read);
                                    IconBitmapDecoder decoder = new IconBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                                    BitmapFrame frame = decoder.Frames[IconNr];
                                    return frame;
                                }
                            }
                            catch (Exception e)
                            {  //We might at least try. This shouldn't stop us from continuing.
                                URI = IconName;
                                IconName = "";  //The icon doesn't work so why try anymore. Hopefully the icon is included in the URI with the icon handle. Will try to extract it in the Icon.ExtractAssociatedIcon(URI)

                            }


                        }
                    }
                    if (File.Exists(URI))
                    {

                        using System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(URI);
                        var handle = (int)sysicon.Handle;
                        return icon = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                    else
                    {
                        var uri = new Uri("pack://application:,,,/Assets/CustomIcons/file.png");
                        return new BitmapImage(uri);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "This should not happen since we check this before...");
                }

                return icon;
            }
        }

        public override string ToString()
        {
            return $"{Name}, {URI}, {Type}, {Command}";
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
        Sleep,
        Link
    }
}