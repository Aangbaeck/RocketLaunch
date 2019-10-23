﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RocketLaunch
{
    internal class InterceptKeys
    {
        #region Delegates

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);


        public static bool IsWin_D(IntPtr wParam, IntPtr lParam)
        {
            if ((int) wParam != WM_KEYDOWN)
                return false;

            var keyInfo = (KbHookParam) Marshal.PtrToStructure(lParam, typeof(KbHookParam));
            if (keyInfo.VkCode != (int) Keys.D) return false;
            return GetAsyncKeyState(Keys.LWin) < 0
                   || GetAsyncKeyState(Keys.RWin) < 0;
        }

        delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct KbHookParam
        {
            public readonly int VkCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr Extra;
        }
    }
}