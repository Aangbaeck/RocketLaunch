using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using MaterialDesignThemes.Wpf;
using BetterStart.Helper;
using Serilog;

namespace Calculator3D.Views.UserControls
{
    /// <summary>
    /// Interaction logic for TitlebarUserCtrl.xaml
    /// </summary>
    public partial class TitlebarUserCtrl
    {
        public TitlebarUserCtrl()
        {
            InitializeComponent();
            MaximizeIcon.Kind = PackIconKind.WindowMaximize;
            MaximizeButton.ToolTip = "Maximize";
        }




        private void StateChanged(object sender, EventArgs e)
        {
            if (sender is Window win)
            {
                MaximizeIcon.Kind = win.WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
            }
        }

        public static readonly DependencyProperty BoundCurrentWindow = DependencyProperty.Register("BoundWindow", typeof(Window), typeof(TitlebarUserCtrl), new PropertyMetadata(WindowPropertyChanged));
        public static readonly DependencyProperty Minimizable = DependencyProperty.Register("EnableMinimize", typeof(bool), typeof(TitlebarUserCtrl), new PropertyMetadata(true, MinPropertyChanged));
        public static readonly DependencyProperty Maximizable = DependencyProperty.Register("EnableMaximize", typeof(bool), typeof(TitlebarUserCtrl), new PropertyMetadata(true, MaxPropertyChanged));
        public static readonly DependencyProperty Closable = DependencyProperty.Register("EnableClosable", typeof(bool), typeof(TitlebarUserCtrl), new PropertyMetadata(true, ClosePropertyChanged));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("TitleProperty", typeof(string), typeof(TitlebarUserCtrl), new PropertyMetadata(TitlePropertyChanged));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("IconProperty", typeof(PackIconKind), typeof(TitlebarUserCtrl), new PropertyMetadata(PackIconKind.Cake, IconPropertyChanged));
        public static readonly DependencyProperty CheckBeforeCloseProperty = DependencyProperty.Register("CheckBeforeCloseProperty", typeof(bool), typeof(TitlebarUserCtrl), new PropertyMetadata(false));

        private static void IconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is PackIconKind icon)) return;
            ((TitlebarUserCtrl)d).TitleIcon.Kind = icon;
        }

        private static void WindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is Window win)) return;
            var bar = ((TitlebarUserCtrl)d);
            win.Deactivated += bar.WindowDeactivated;
            win.Activated += bar.WindowActivated;
            win.StateChanged += bar.StateChanged;
            win.Loaded += WindowLoaded;
            win.Closing += WindowClosing;
            bar.LocalWindow = win;
        }

        public Window LocalWindow
        {
            get { return _localWindow; }
            set
            {
                _localWindow = value;
                BoundWindow?.LoadPlacement();
            }
        }

        //This method is save the actual position of the window to file "WindowName.pos"
        private static void WindowClosing(object window, CancelEventArgs ee)
        {
            try
            {
                ((Window)window).SavePlacement();
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not save window position.");
            }
        }
        ///This method is load the actual position of the window from the file
        private static void WindowLoaded(object window, RoutedEventArgs ee)
        {
            try
            {
                ((Window)window).LoadPlacement();
            }
            catch
            {
                // ignored
            }
        }

        private static void TitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is string s)) return;
            ((TitlebarUserCtrl)d).TitleText.Text = s;
        }
        private static void ClosePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is bool b)) return;
            ((TitlebarUserCtrl)d).CloseButton.Visibility = b ? Visibility.Visible : Visibility.Hidden;
        }
        private static void MinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is bool b)) return;
            ((TitlebarUserCtrl)d).MinimizeButton.Visibility = b ? Visibility.Visible : Visibility.Hidden;
        }
        private static void MaxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is bool b)) return;
            ((TitlebarUserCtrl)d).MaximizeButton.Visibility = b ? Visibility.Visible : Visibility.Hidden;
        }



        public Window BoundWindow
        {
            get
            {
                return (Window)GetValue(BoundCurrentWindow);
            }
            set
            {
                SetValue(BoundCurrentWindow, value);

            }
        }
        public bool EnableMinimize { get { return (bool)GetValue(Minimizable); } set { SetValue(Minimizable, value); } }
        public bool EnableMaximize { get { return (bool)GetValue(Maximizable); } set { SetValue(Maximizable, value); } }
        public bool EnableClosable { get { return (bool)GetValue(Closable); } set { SetValue(Closable, value); } }
        public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }
        public PackIconKind Icon { get { return (PackIconKind)GetValue(IconProperty); } set { SetValue(IconProperty, value); } }

        public bool CheckBeforeClose { get { return (bool)GetValue(CheckBeforeCloseProperty); } set { SetValue(CheckBeforeCloseProperty, value); } }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            BoundWindow.WindowState = WindowState.Minimized;
        }

        //These mouse methods is used for normal window behavour and still it's a borderless stylable window
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (BoundWindow.ResizeMode != ResizeMode.CanResize &&
                    BoundWindow.ResizeMode != ResizeMode.CanResizeWithGrip)
                {
                    return;
                }

                BoundWindow.WindowState = BoundWindow.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                _mRestoreForDragMove = BoundWindow.WindowState == WindowState.Maximized;
                BoundWindow.DragMove();
            }
        }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mRestoreForDragMove)
            {
                _mRestoreForDragMove = false;

                var point = PointToScreen(e.MouseDevice.GetPosition(this));

                BoundWindow.Left = point.X - (BoundWindow.RestoreBounds.Width * 0.5);
                BoundWindow.Top = point.Y;
                BoundWindow.WindowState = WindowState.Normal;

                BoundWindow.DragMove();


            }
        }
        private void MaximizeRestoreWindow(object sender, RoutedEventArgs e)
        {
            if (BoundWindow.WindowState == WindowState.Maximized)
            {
                BoundWindow.WindowState = WindowState.Normal;
                MaximizeButton.ToolTip = "Maximize";
            }
            else if (BoundWindow.WindowState == WindowState.Normal)
            {
                BoundWindow.WindowState = WindowState.Maximized;
                MaximizeButton.ToolTip = "Restore";
            }
        }
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mRestoreForDragMove = false;
        }
        private bool _mRestoreForDragMove;
        private Window _localWindow;


        //These methods is to make the title text a bit faded out when window is not active (e.g. how visual studio looks)
        private void WindowDeactivated(object sender, EventArgs e)
        {
            TitleIcon.Opacity = 0.4;
            TitleText.Opacity = 0.4;
            Border.Opacity = 0.4;
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            TitleIcon.Opacity = 1;
            TitleText.Opacity = 0.9;
            Border.Opacity = 1;
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (CheckBeforeClose)
                DialogHost.IsOpen = true;
            else
            {
                try
                {
                    BoundWindow.Close();  //Close this window (Main)
                }
                catch
                {
                    Log.Error("Could not close window properly");
                }
            }
        }

        private void OnDialogClosing(object sender, DialogClosingEventArgs eventargs)
        {
            if (!(bool)eventargs.Parameter) return;
            try
            {
                BoundWindow.Close();  //Close this window (Main)
            }
            catch
            {
                Log.Error("Could not close window properly");
            }
        }
    }
}
