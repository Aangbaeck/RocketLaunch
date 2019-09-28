using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using BetterStart.Helper;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace BetterStart
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        static App()
        {
            
            //using (var kh = new KeyboardHook(KeyboardHook.Parameters.AllowAltTab))
            //{
            //kh.KeyIntercepted += new KeyboardHook.KeyboardHookEventHandler(kh_KeyIntercepted);
            //void Current_Exit(object sender, ExitEventArgs e)
            //{
            //    DetachKeyboardHook();
            //}
            //Application.Current.Exit += new ExitEventHandler(Current_Exit);
            try
            {
                _hookID = InterceptKeys.SetHook(_proc);
            }
            catch
            {
                DetachKeyboardHook();
            }


            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(Common.LogfilesPath + "Logfile.log", rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 20000000, retainedFileCountLimit: 5)
                    .WriteTo.Logger(lc =>
                        lc.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.StealthConsoleSink())
                    .CreateLogger();
                AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                //We only want one application to run.
                Process current = Process.GetCurrentProcess();
                // get all the processes with current process name
                Process[] processes = Process.GetProcessesByName(current.ProcessName);
                foreach (Process process in processes)
                {
                    //Ignore the current process  
                    if (process.Id != current.Id)
                    {
                        process.Kill();
                    }
                }


                DispatcherHelper.Initialize();
            //}

            
        }
       

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.Error((e.ExceptionObject as Exception), "CurrentDomain_UnhandledException!!!");
        }
       
        /// <summary>
        /// Detach the keyboard hook; call during shutdown to prevent calls as we unload
        /// </summary>
        private static void DetachKeyboardHook()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        private Keys LastPressed { get; set; }
        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam.ToString() == "256")
                    return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
                bool control = (WinForms.Control.ModifierKeys & Keys.Control) != 0;

                
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                
                bool win = key == Keys.LWin;

                //if (alt && key == Keys.F4)
                //{
                //    Application.Current.Shutdown();
                //    return (IntPtr)1; // Handled.
                //}

                if (!AllowKeyboardInput(win, control, key))
                {
                    return (IntPtr)1; // this happens if it's not allowed.
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>Determines whether the specified keyboard input should be allowed to be processed by the system.</summary>
        /// <remarks>Helps block unwanted keys and key combinations that could exit the app, make system changes, etc.</remarks>
        public static bool AllowKeyboardInput(bool win, bool control, Keys key)
        {
            // Disallow various special keys.
            //if (key <= Keys.Back || key == Keys.None ||
            //    key == Keys.Menu || key == Keys.Pause ||
            //    key == Keys.Help)
            //{
            //    return false;
            //}
            //if()
            
            //Log.Debug($"{key.ToString()} + {win}" );
            if (key == Keys.LWin || (control && key == Keys.Escape))
            {
                   
                Messenger.Default.Send<bool>(true,MessengerID.WindowKeyPressed);
                return false;
            }

            // Disallow ranges of special keys.
            // Currently leaves volume controls enabled; consider if this makes sense.
            // Disables non-existing Keys up to 65534, to err on the side of caution for future keyboard expansion.
            //    if ((key >= Keys.LWin && key <= Keys.Sleep) ||
            //    (key >= Keys.KanaMode && key <= Keys.HanjaMode) ||
            //    (key >= Keys.IMEConvert && key <= Keys.IMEModeChange) ||
            //    (key >= Keys.BrowserBack && key <= Keys.BrowserHome) ||
            //    (key >= Keys.MediaNextTrack && key <= Keys.LaunchApplication2) ||
            //    (key >= Keys.ProcessKey && key <= (Keys)65534))
            //{
            //    return false;
            //}

            // Disallow specific key combinations. (These component keys would be OK on their own.)
            //if ((alt && key == Keys.Tab) ||
            //    (alt && key == Keys.Space) ||
            //    (control && key == Keys.Escape))
            //{
            //    return false;
            //}

            // Allow anything else (like letters, numbers, spacebar, braces, and so on).
                return true;
        }


    }
    public static class StealthConsoleSinkExtensions
    {
        public static LoggerConfiguration StealthConsoleSink(
            this LoggerSinkConfiguration loggerConfiguration,
            IFormatProvider fmtProvider = null)
        {
            return loggerConfiguration.Sink(new StealthConsoleSink(fmtProvider));
        }
    }

    public class StealthConsoleSink : ILogEventSink
    {
        IFormatProvider _formatProvider;

        public StealthConsoleSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            Messenger.Default.Send(logEvent.RenderMessage(_formatProvider), MessengerID.LogFrontEndMessage);
        }
    }
}
