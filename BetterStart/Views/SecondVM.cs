﻿using System.ComponentModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using BetterStart.Model;

namespace BetterStart.Views
{
    public class SecondVM : ViewModelBase
    {
        private string _propertyName;

        public SecondVM(DataService d)
        {
            D = d;
            D.PropertyChanged += DataUpdatedInDataService;
        }

        private void DataUpdatedInDataService(object sender, PropertyChangedEventArgs e)
        {
            PropertyName = e.PropertyName;
        }

        public DataService D { get; set; }

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; RaisePropertyChanged(); }
        }
    }
}