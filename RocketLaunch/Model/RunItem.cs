using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using ProtoBuf;
using RocketLaunch.Services;
using RocketLaunch.Views;
using Serilog;
using PixelFormat = System.Windows.Media.PixelFormat;

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
                    switch (Type)
                    {
                        case ItemType.Directory:
                        {
                            var uri = new Uri("pack://application:,,,/Assets/CustomIcons/folder.png");
                            return new BitmapImage(uri);
                        }
                        case ItemType.ControlPanelSetting:
                        case ItemType.RunDialog:
                        case ItemType.Hibernate:
                        case ItemType.LockComputer:
                        case ItemType.RestartComputer:
                        case ItemType.LogOffComputer:
                        case ItemType.TurnOffComputer:
                        case ItemType.Sleep:
                        {
                            var uri = new Uri("pack://application:,,,/Assets/CustomIcons/" + IconName);
                            var service = ServiceLocator.Current.GetInstance<SettingsService>();
                            var bmp = new BitmapImage(uri);
                            if (!service.Settings.DarkTheme)
                            {
                                var bmpTemp = Invert(bmp);
                                return bmpTemp;
                            }
                            return bmp ;
                        }
                        case ItemType.Win10App when IconName != null:
                        {
                            var uri = new Uri(IconName);
                            return new BitmapImage(uri);
                        }
                        case ItemType.Link:
                        {
                            if (IconName != null)
                            {
                                try
                                {
                                    if (File.Exists(IconName))
                                    {
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

                            break;
                        }
                    }

                    if (File.Exists(URI))  //else
                    {

                        using System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(URI);
                        if (sysicon != null)
                        {
                            var handle = (int)sysicon.Handle;
                        }
                        return Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
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


        public static BitmapSource Invert(BitmapSource source)
        {
            // Calculate stride of source
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;

            // Create data array to hold source pixel data
            int length = stride * source.PixelHeight;
            byte[] data = new byte[length];

            // Copy source image pixels to the data array
            source.CopyPixels(data, stride, 0);

            // Change this loop for other formats
            for (int i = 0; i < length; i += 4)
            {
                data[i] = (byte)(255 - data[i]); //R
                data[i + 1] = (byte)(255 - data[i + 1]); //G
                data[i + 2] = (byte)(255 - data[i + 2]); //B
                //data[i + 3] = (byte)(255 - data[i + 3]); //A
            }

            // Create a new BitmapSource from the inverted pixel buffer
            return BitmapSource.Create(
                source.PixelWidth, source.PixelHeight,
                source.DpiX, source.DpiY, source.Format,
                null, data, stride);
        }
        private unsafe void InvertImage(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int* bytes = (int*)data.Scan0;
            for (int i = w * h - 1; i >= 0; i--)
                bytes[i] = ~bytes[i];
            bmp.UnlockBits(data);
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
