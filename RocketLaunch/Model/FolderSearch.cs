using GalaSoft.MvvmLight;

namespace RocketLaunch.Model
{
    public class FolderSearch : ViewModelBase
    {
        private bool _includeFoldersInSearch = true;
        private int _nrOfFiles;
        private string _path;
        private string _searchPattern = "*.*";
        private bool _searchSubFolders = true;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged();
            }
        }

        public string SearchPattern
        {
            get { return _searchPattern; }
            set
            {
                _searchPattern = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Search the subfolders of the current folder
        /// </summary>
        public bool SearchSubFolders
        {
            get { return _searchSubFolders; }
            set
            {
                _searchSubFolders = value;
                RaisePropertyChanged();
            }
        }

        public bool IncludeFoldersInSearch
        {
            get { return _includeFoldersInSearch; }
            set
            {
                _includeFoldersInSearch = value;
                RaisePropertyChanged();
            }
        }

        public int NrOfFiles
        {
            get { return _nrOfFiles; }
            set
            {
                _nrOfFiles = value;
                RaisePropertyChanged();
            }
        }
    }
}