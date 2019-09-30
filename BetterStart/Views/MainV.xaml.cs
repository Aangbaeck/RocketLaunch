using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using BetterStart.Helper;
using BetterStart.Model;
using Serilog;
using Serilog.Formatting.Json;
using Path = System.IO.Path;
using static BetterStart.App;
using BetterStart.Services;

namespace BetterStart.Views
{
    public partial class MainV
    {
        public MainV()
        {
            Application.Current.DispatcherUnhandledException += ThreadStuffUI;
            SimpleIoc.Default.Register<SettingsService>();
            S = SimpleIoc.Default.GetInstance<SettingsService>();

            Log.Information("STARTING APPLICATION...");
            InitializeComponent();
            Messenger.Default.Register<Type>(this, MessengerID.MainWindowV, OpenAnotherWindow);
            Messenger.Default.Register<KeyState>(this, MessengerID.KeyPressed, HideShowWindow);
            Closing += (s, e) =>
            {
                Log.Information("CLOSING APPLICATION...");
                this.SavePlacement();  //Saves this windows position
                var listOfWindowsToOpenNextTime = new List<Type>();
                var windows = Application.Current.Windows;  //Close every window individually to save their position
                foreach (Window window in windows)
                {
                    var t = window.GetType();
                    if (window == this || t.Name == "AdornerLayerWindow") continue;  //We will close this window below
                    listOfWindowsToOpenNextTime.Add(window.GetType());
                    window.Close();
                }

                var startWindows = JsonConvert.SerializeObject(listOfWindowsToOpenNextTime);
                S.WindowsToOpenAtStart = startWindows;
                S.SaveSettings();
                ViewModelLocator.Cleanup();
            };

        }

        private void HideShowWindow(KeyState state)
        {
            if (state.Key == System.Windows.Forms.Keys.LWin && !state.IsDown)
            {
                if (this.WindowState == WindowState.Minimized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Minimized;
            }
        }

        public SettingsService S { get; set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.LoadPlacement();  //Sets the last position of the window
        }

        private void OpenAnotherWindow(Type window)
        {
            if (typeof(SecondV) == window)
            {
                if (IsWindowOpen<SecondV>())  //If window is already open, why open another?
                    Application.Current.Windows.OfType<SecondV>().First().Activate(); //Attempts to bring the current window to the foreground
                else
                    new SecondV() { Owner = this }.Show();
            }
            //else if (typeof(AnotherV) == window)
            //{
            //    if (IsWindowOpen<AnotherV>())
            //        Application.Current.Windows.OfType<AnotherV>().First().Activate(); //Attempts to bring the current window to the foreground
            //    else
            //        new AnotherV() { Owner = this }.Show();
            //}
        }


        /// <summary>
        /// This method will check for custom windows as well by specifying T to window type
        /// </summary>
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var startWindows = JsonConvert.DeserializeObject<List<Type>>(S.WindowsToOpenAtStart);
                foreach (var w in startWindows)
                {
                    try
                    {
                        OpenAnotherWindow(w);
                    }
                    catch
                    {
                        //ignore
                    }

                }
            }
            catch
            {
                Log.Error("Could not read window positions setting.");
            }

            SearchTextBox.Focusable = true;
            Keyboard.Focus(SearchTextBox);
        }

        /// <summary>
        /// This often finds weird threading errors in the UI.
        /// </summary>
        private void ThreadStuffUI(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Some UI Error!");
        }



        //These mouse methods is used for normal window behavour and still it's a borderless stylable window
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    if (this.ResizeMode != ResizeMode.CanResize &&
            //        this.ResizeMode != ResizeMode.CanResizeWithGrip)
            //    {
            //        return;
            //    }

            //    this.WindowState = this.WindowState == WindowState.Maximized
            //        ? WindowState.Normal
            //        : WindowState.Maximized;
            //}
            //else
            //{
            _mRestoreForDragMove = this.WindowState == WindowState.Maximized;
            this.DragMove();
            //}
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mRestoreForDragMove)
            {
                _mRestoreForDragMove = false;

                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                this.Left = point.X - (this.RestoreBounds.Width * 0.5);
                this.Top = point.Y;
                this.WindowState = WindowState.Normal;

                this.DragMove();


            }
        }
        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                this.ToolTip = "Maximize";
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                this.ToolTip = "Restore";
            }
        }
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mRestoreForDragMove = false;
        }
        private bool _mRestoreForDragMove;

        private void WindowStateChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
                LastWindowState = this.WindowState;
        }

        public WindowState LastWindowState { get; set; }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            Window window = (Window)sender;
            window.Topmost = true;
        }

    }
}