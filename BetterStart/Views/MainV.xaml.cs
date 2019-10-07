using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using Serilog;
using Serilog.Formatting.Json;
using Path = System.IO.Path;
using static RocketLaunch.App;
using RocketLaunch.Services;

namespace RocketLaunch.Views
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
            Messenger.Default.Register<KeyState>(this, MessengerID.WinKeyPressed, HideShowWindow);
            Messenger.Default.Register<bool>(this, MessengerID.HideWindow, HideWindow);
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
                S.Settings.WindowsToOpenAtStart = startWindows;
                ViewModelLocator.Cleanup();
            };

            

        }
        private void HideWindow(bool b)
        {
            //((Storyboard)FindResource("FadeOut")).Begin(this);
            //Wait(300);
            this.WindowState = WindowState.Minimized;
        }

        private void HideShowWindow(KeyState state)
        {

            if (state.Key == System.Windows.Forms.Keys.LWin && !state.IsDown)
            {
                ToogleWindowState(null, null);
            }
        }
        private void Wait(double milliseconds)
        {
            var frame = new DispatcherFrame();
            var t = Task.Run(
                async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
                    frame.Continue = false;
                });

            Dispatcher.PushFrame(frame);
            t.Wait();
        }

        public SettingsService S { get; set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.LoadPlacement();  //Sets the last position of the window
        }

        private void OpenAnotherWindow(Type window)
        {
            //if (typeof(SettingsV) == window)
            //{
            //    if (IsWindowOpen<SettingsV>())  //If window is already open, why open another?
            //        Application.Current.Windows.OfType<SettingsV>().First().Activate(); //Attempts to bring the current window to the foreground
            //    else
            //        new SettingsV() { Owner = this }.Show();
            //}
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
                var startWindows = JsonConvert.DeserializeObject<List<Type>>(S.Settings.WindowsToOpenAtStart);
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

            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;

        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && e.IsDown)
                ToogleWindowState(null, null);
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
            _mRestoreForDragMove = this.WindowState == WindowState.Maximized;
            this.DragMove();
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
            Messenger.Default.Send<bool>(true,MessengerID.WindowDeactivated);
            
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void WinOnKeyDown(object sender, KeyEventArgs e)
        {
            SearchTextBox.Focus();
        }


        private void ToogleWindowState(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                //Log.Debug("In");
                //this.Opacity = 0;
                //((Storyboard)FindResource("FadeIn")).Begin(this);
                this.WindowState = WindowState.Normal;
                //Wait(300);
                SearchTextBox.SelectAll();
            }
            else
            {
                //Log.Debug("Out");
                //((Storyboard) FindResource("FadeOut")).Begin(this);
                //Wait(300);
                this.WindowState = WindowState.Minimized;
            }
        }
    }
}