using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace RocketLaunch.Model
{
    public class FolderSearch : ViewModelBase
    {
        private bool _searchSubFolders;
        private string _searchPattern;
        private string _path;
        private int _nrOfFiles;

        public string Path
        {
            get { return _path; }
            set { _path = value; RaisePropertyChanged(); }
        }

        public string SearchPattern
        {
            get { return _searchPattern; }
            set { _searchPattern = value; RaisePropertyChanged(); }
        }

        public bool SearchSubFolders
        {
            get { return _searchSubFolders; }
            set { _searchSubFolders = value; RaisePropertyChanged(); }
        }

        public int NrOfFiles
        {
            get { return _nrOfFiles; }
            set
            {
                _nrOfFiles = value; RaisePropertyChanged();
            }
        }
    }
}
