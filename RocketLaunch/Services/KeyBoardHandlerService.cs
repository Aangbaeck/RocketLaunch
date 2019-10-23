using System;
using System.Runtime.InteropServices;
using AutoHotkey.Interop;
using GalaSoft.MvvmLight.Messaging;

namespace RocketLaunch.Services
{
    public class KeyBoardHandlerService
    {
        private readonly AHKDelegate ahkDelegate;
        
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void AHKDelegate([MarshalAs(UnmanagedType.LPWStr)] string s);
    }
}