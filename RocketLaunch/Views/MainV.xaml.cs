using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MaterialDesignThemes.Wpf;
using RocketLaunch.Helper;
using RocketLaunch.Model;
using RocketLaunch.Services;
using Serilog;

namespace RocketLaunch.Views
{
    public partial class MainV
    {
        private bool _mRestoreForDragMove;

        public MainV()
        {
            Application.Current.DispatcherUnhandledException += ThreadStuffUI;
            SimpleIoc.Default.Register<SettingsService>();
            S = SimpleIoc.Default.GetInstance<SettingsService>();

            Log.Information("STARTING APPLICATION...");
            InitializeComponent();
            Messenger.Default.Register<bool>(this, MessengerID.WinKeyPressed, HideShowWindow);
            Messenger.Default.Register<bool>(this, MessengerID.HideWindow, HideWindow);
            Closing += (s, e) =>
            {
                Log.Information("CLOSING APPLICATION...");
                this.SavePlacement(); //Saves this windows position
                ViewModelLocator.Cleanup();
            };

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata {DefaultValue = 60});
        }

        public SettingsService S { get; set; }

        public WindowState LastWindowState { get; set; }

        private void HideWindow(bool b)
        {
            //((Storyboard)FindResource("FadeOut")).Begin(this);
            //Wait(300);
            Hide();
            SearchTextBox.SelectAll();
            Hide_PopupToolTip(null, null);
            WindowState = WindowState.Minimized;
        }

        private void HideShowWindow(bool state)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => { ToogleWindowState(null, null); }));
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.LoadPlacement(); //Sets the last position of the window
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focusable = true;
            Keyboard.Focus(SearchTextBox);

            var window = GetWindow(this);
            if (window != null) window.KeyDown += HandleKeyPress;
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
            if (e.Source is Card)
            {
                _mRestoreForDragMove = WindowState == WindowState.Maximized;
                DragMove();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mRestoreForDragMove)
            {
                _mRestoreForDragMove = false;
                var point = PointToScreen(e.MouseDevice.GetPosition(this));
                Left = point.X - (RestoreBounds.Width * 0.5);
                Top = point.Y;
                WindowState = WindowState.Normal;
                DragMove();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mRestoreForDragMove = false;
        }

        private void WindowStateChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                LastWindowState = WindowState;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Messenger.Default.Send<bool>(true, MessengerID.WindowDeactivated);

            Window window = (Window) sender;
            window.Topmost = true;
        }

        private void WinOnKeyDown(object sender, KeyEventArgs e)
        {
            SearchTextBox.Focus();
        }

        private void ToogleWindowState(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)

            {
                Activate();
                //this.Focus();
                //Log.Debug("In");d
                //this.Opacity = 0;
                //((Storyboard)FindResource("FadeIn")).Begin(this);
                WindowState = WindowState.Normal;
                //Wait(300);
                Show();
                SearchTextBox.SelectAll();
                SearchTextBox.Focus();
            }
            else
            {
                //Log.Debug("Out");
                //((Storyboard) FindResource("FadeOut")).Begin(this);
                //Wait(300);
                HideWindow(true);
            }
        }

        private void Show_PopupToolTip(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Hide_PopupToolTip(null, null);
                ListViewItem listViewItem = sender as ListViewItem;

                MyToolTip.PlacementTarget = listViewItem;
                MyToolTip.Placement = PlacementMode.MousePoint;
                MyToolTip.IsOpen = true;
            }
            else
            {
                Hide_PopupToolTip(null, null);
            }
        }

        private void Hide_PopupToolTip(object sender, MouseEventArgs e)
        {
            MyToolTip.IsOpen = false;
        }

        private void HidePopupAndResetFocusOnTextBoox(object sender, MouseEventArgs e)
        {
            MyToolTip.IsOpen = false;
            SearchTextBox.Focus();
        }

        private void Hide_Window(object sender, MouseButtonEventArgs e)
        {
            HideWindow(true);
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth =
                listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            var col1 = 0.92;
            var col2 = 0.07;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void MainV_OnLostFocus(object sender, RoutedEventArgs e)
        {
            //HideWindow(true);
        }
    }
}