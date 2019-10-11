using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using RocketLaunch.Helper;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;
using RocketLaunch.Services;
using CommonServiceLocator;
using AutoMapper;



namespace RocketLaunch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        static App()
        {
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
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.Error((e.ExceptionObject as Exception), "CurrentDomain_UnhandledException!!!");
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
