﻿using System.ComponentModel;
using System.Reflection;
using GalaSoft.MvvmLight;
using BetterStart.Model;

namespace BetterStart.Views
{
    public class SecondVM : ViewModelBase
    {
        private string _propertyName;

        public SecondVM()
        {
            
        
        }

        private void DataUpdatedInDataService(object sender, PropertyChangedEventArgs e)
        {
            PropertyName = e.PropertyName;
        }

        

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; RaisePropertyChanged(); }
        }
    }
}