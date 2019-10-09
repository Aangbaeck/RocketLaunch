using GalaSoft.MvvmLight.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using static RocketLaunch.App;

namespace RocketLaunch.Services
{
    public class KeyBoardHandlerService
    {
        public KeyBoardHandlerService()
        {
            //Messenger.Default.Register<KeyState>(this, MessengerID.KeyPressed, CheckIfOtherKeyFollows);
            InputSimulator = new InputSimulator();
            SW.Start();
        }
        public KeyState LastState { get; set; } = new KeyState();

        public InputSimulator InputSimulator { get; }
        public bool LWinDown { get; set; }
        public bool AllowThroughWinDown { get; set; } = false;
        public bool AllowThroughWinUp { get; set; } = false;
        public Stopwatch SW { get; set; } = new Stopwatch();
        public void Trigger()
        {
            //Log.Debug($"Send virtual LWin+{LastState}");
            AllowThroughWinDown = true;
            AllowThroughWinUp = true;
            var tempKey = LastState.Key;
            //Log.Debug("Send Down LWIN");
            InputSimulator.Keyboard.KeyDown(VirtualKeyCode.LWIN);
            //Log.Debug($"Send Down {tempKey}");
            InputSimulator.Keyboard.KeyDown((VirtualKeyCode)tempKey);
            //Log.Debug("Send Up LWIN");
            InputSimulator.Keyboard.KeyUp(VirtualKeyCode.LWIN);
            //Log.Debug($"Send Up {tempKey}");
            InputSimulator.Keyboard.KeyUp(((VirtualKeyCode)tempKey));
        }

        public void CheckForCombo()
        {
            //Log.Debug("Checking combo");
            if (LWinDown && LastState.Key != Keys.LWin && LastState.IsDown && SW.ElapsedMilliseconds > 200)
            {
                //Log.Debug($"Trigger combo {LastState.Key}");
                SW.Restart();
                Trigger();
            }
        }
        
    }
    public class KeyState
    {
        public bool IsDown { get; set; }
        //public bool IsReleased => !IsPressed;

        public Keys Key { get; set; }
    }
}
