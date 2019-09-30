using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;

namespace BetterStart.Indexing
{


    public class ItemInfo : ViewModelBase
    {
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FileName));
                RaisePropertyChanged(nameof(FileNameWithoutExtension));
            }
        }
        public string FileName => System.IO.Path.GetFileName(Path);
        public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Path);

        public ImageSource Icon
        {
            get
            {
                BitmapSource icon = null;
                try
                {
                    using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(Path))
                    {
                        if (sysicon != null)
                            icon = Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                catch (Exception)
                {
                    
                }



                return icon;
            }

        }





    }
    
}
