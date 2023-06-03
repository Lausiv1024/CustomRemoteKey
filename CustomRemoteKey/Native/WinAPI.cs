using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CustomRemoteKey
{
    internal class WinAPI
    {
        private const int KEYEVENTF_EXTENDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;
        private const int KEYEVENTF_SCANCODE = 8;
        private const int KEYEVENTF = 4;

        private const int MAPVK_VK_TO_VSC = 0;

        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void SendInput(int uInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        public extern static int MapVirtualKey(int wCode, int wMapType);

        [DllImport("user32.dll")]
        public extern static bool BlockInput(bool fBlockIt);

        [DllImport("user32.dll")]
        public extern static short GetKeyState(int nVirtKey);

        public static void KeyDown(int key)
        {
            KeyOperation(key, true);
        }

        public static void KeyUp(int key) => KeyOperation(key, false);

        public static void KeyOperation(int key, bool keyState)
        {
            INPUT[] inputs = new INPUT[1];
            int vsc = MapVirtualKey(key, MAPVK_VK_TO_VSC);
            inputs[0] = new INPUT();
            inputs[0].type = 1;
            inputs[0].ui.keyboard.wVk = (short)key;
            inputs[0].ui.keyboard.wScan = (short)vsc;
            inputs[0].ui.keyboard.dwFlags = keyState ? 0 : KEYEVENTF_KEYUP;
            inputs[0].ui.keyboard.time = 0;
            inputs[0].ui.keyboard.dwExtraInfo = IntPtr.Zero;
            SendInput(inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
        }

        public static void SendInputKeyPress(int key)
        {
            INPUT[] inputs = new INPUT[2];

            int vsc = MapVirtualKey(key, MAPVK_VK_TO_VSC);

            inputs[0] = new INPUT();
            inputs[0].type = 1;
            inputs[0].ui.keyboard.wVk = (short)key;
            inputs[0].ui.keyboard.wScan = (short)vsc;
            inputs[0].ui.keyboard.dwFlags = 0;
            inputs[0].ui.keyboard.time = 2000;
            inputs[0].ui.keyboard.dwExtraInfo = IntPtr.Zero;

            inputs[1] = new INPUT();
            inputs[1].type = 1;
            inputs[1].ui.keyboard.wVk = (short)key;
            inputs[1].ui.keyboard.wScan = (short)vsc;
            inputs[1].ui.keyboard.dwFlags = KEYEVENTF_KEYUP;
            inputs[1].ui.keyboard.time = 0;
            inputs[1].ui.keyboard.dwExtraInfo = IntPtr.Zero;

            SendInput(inputs.Length, inputs, Marshal.SizeOf(inputs[0]));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int Time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public short wVk;
        public short wScan;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_UNION
    {
        [FieldOffset(0)] public MOUSEINPUT mouse;
        [FieldOffset(0)] public KEYBDINPUT keyboard;
        [FieldOffset(0)] public HARDWAREINPUT hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public int type;
        public INPUT_UNION ui;
    }
}
