using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Management;

namespace RocketLaunch.Model
{
    public static class WindowHelper
    {
        public static void ExecuteAsAdminNewInstance(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            if (Path.GetExtension(fileName) == ".exe")
                proc.StartInfo.Verb = "runas"; //This forces it to use admin rights
            proc.Start();
        }

        public static void BringProcessToFrontOrStartIt(string fileName, string arguments, bool asAdmin)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = true;
            if (asAdmin && Path.GetExtension(fileName) == ".exe")
                proc.StartInfo.Verb = "runas";  //This forces it to use admin rights
            BringProcessToFrontOrStartIt(proc, asAdmin);
        }
        private static void BringProcessToFrontOrStartIt(Process process, bool asAdmin)
        {
            try
            {
                
                int counter = 0;
                
                   var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
                        using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                        using (var results = searcher.Get())
                        {
                            var query = from ppp in Process.GetProcesses()
                                join mo in results.Cast<ManagementObject>()
                                    on ppp.Id equals (int)(uint)mo["ProcessId"]
                                select new
                                {
                                    Process = ppp,
                                    Path = (string)mo["ExecutablePath"],
                                    CommandLine = (string)mo["CommandLine"],
                                };
                            foreach (var item in query)
                            {
                                if (item.Path == process.StartInfo.FileName)
                                {
                                    IntPtr handle = item.Process.MainWindowHandle;
                                    if (IsIconic(handle))
                                    {
                                        ShowWindow(handle, SW_RESTORE);
                                    }

                                    SetForegroundWindow(handle);
                                    counter++;
                                }
                            }
                        }



                        
                    
                
                if(counter==0)
                //There was no process already running, just start it normally
                    process.Start();
                
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not start process");

            }


        }
        
        const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }
}