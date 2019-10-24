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
        [ProtoMember(5)] public string IconName { get; set; } //manually customized icons
        [ProtoMember(6)] public string IconBackGround { get; set; } = "Transparent"; //manually customized icons

        [ProtoMember(7)]
        public string
            URI
        {
            get;
            set;
        } //The file path or website or specific text that should be written underneath the Name in the UI.

        [ProtoMember(8)] public string Arguments { get; set; } //The arguments to run with the URI
        [ProtoMember(9)] public string Tooltip { get; set; } //Something interesting to show about the file. Links contain descriptions that can be interesting to show here.
        [ProtoMember(10)] public int RunNrOfTimes { get; set; } = 0;


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

                    if (Type == ItemType.Link)
                    {
                        var uri = URI;
                        if (IconName != null)
                        {
                            try
                            {
                                var split = IconName.Split(',');
                                var iconName = split[0];
                                int nr = Convert.ToInt32(split[1]);
                                if (File.Exists(iconName))
                                {
                                    Stream iconStream = new FileStream(iconName, FileMode.Open, FileAccess.Read);
                                    IconBitmapDecoder decoder = new IconBitmapDecoder(iconStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

                                    BitmapFrame frame = decoder.Frames[nr];

                                    return frame;
                                }
                            }
                            catch (Exception e)
                            {
                                //We might at least try. This shouldn't stop us from continuing.
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