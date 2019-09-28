using GalaSoft.MvvmLight;

namespace BetterStart.Model
{
    public class DataService : ViewModelBase
    {
        private string _title = "BetterStart V1.8 20190423";

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }
    }
}