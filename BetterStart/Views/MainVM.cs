using System;
using System.IO;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using BetterStart.Helper;
using BetterStart.Model;
using Serilog;

namespace BetterStart.Views
{
    public class MainVM : ViewModelBase
    {


        public readonly DataService D;
        
        public RelayCommand OpenNewWindowCmd => new RelayCommand(() => { Messenger.Default.Send(typeof(SecondV), MessengerID.MainWindowV); });
        public string SearchString { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class. IOC
        /// </summary>
        public MainVM()
        {
         
        }

        
    }
}