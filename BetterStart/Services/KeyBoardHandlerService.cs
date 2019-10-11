using GalaSoft.MvvmLight.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsInput;
using WindowsInput.Native;
using AutoHotkey.Interop;
using Gma.System.MouseKeyHook;
using static RocketLaunch.App;

namespace RocketLaunch.Services
{
    public class KeyBoardHandlerService
    {
        private IKeyboardMouseEvents _globalHook;

        public KeyBoardHandlerService()
        {
            //https://github.com/amazing-andrew/AutoHotkey.Interop

            ahkDelegate = AHKCallback;
            void CallBackFunction()
            {
                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(ahkDelegate);
                var ahk = AutoHotkeyEngine.Instance;
                ahk.ExecRaw(@"~LWin Up::
      DllCall(" + ptr + @", ""Str"", ""WinDown"")
      return");
            }
            CallBackFunction();

        }
        private void AHKCallback(string s)
        {
            Messenger.Default.Send<bool>(true, MessengerID.WinKeyPressed);
        }
        private AHKDelegate ahkDelegate;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void AHKDelegate([MarshalAs(UnmanagedType.LPWStr)]string s);
        
        public void ReleaseControlOfWinKey()
        {
            var ahk = AutoHotkeyEngine.Instance;
            ahk.Suspend();
        }
        public void TakeControlOfWinKey()
        {
            var ahk = AutoHotkeyEngine.Instance;
            ahk.UnSuspend();
        }






    }

}
