using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Windows.Threading;

namespace CustomRemoteKey.Behaviours
{
    internal class InputHotKey : BehaviourBase
    {
        [JsonIgnore]
        private TextBlock KeyName { get; set; }
        [JsonIgnore]
        private Button HotKeySettingButton { get; set; }
        [JsonIgnore]
        private Button SetSpecialKey { get; set; }
        [JsonIgnore]
        DispatcherTimer timer = new DispatcherTimer();
        private int rendaCount = 0;
        
        public bool HasWin { get; set; }
        public bool HasShift { get; set; }
        public bool HasCtrl { get; set; }
        public bool HasAlt { get; set; }
        public int Key { get; set; }


        private bool AcceptingKeyInput = false;
        public InputHotKey() : base("CustomHotKey", "ホットキー")
        {
            Key = 0;
            HotKeySettingButton = new Button();
            KeyName = new TextBlock();
            timer = new DispatcherTimer();
            HotKeySettingButton.Content = "ボタンを押して設定変更";
            HotKeySettingButton.Click += (s, e) =>
            {
                AcceptingKeyInput = !AcceptingKeyInput;

                if (AcceptingKeyInput)
                {
                    Key = 0;
                    HasCtrl = false;
                    HasAlt = false;
                    HasShift = false;
                    HasWin = false;
                    MainWindow.Instance.IsInHotKeySetting = true;
                    HotKeySettingButton.Content = "変更中(もう一度押してキャンセル)";
                } else
                {
                    HotKeySettingButton.Content = "ボタンを押して設定変更";
                    MainWindow.Instance.IsInHotKeySetting = false;
                }
            };
            HotKeySettingButton.Unloaded += (s, e) =>
            {
                AcceptingKeyInput = false;
                HotKeySettingButton.Content = "ボタンを押して設定変更";
                MainWindow.Instance.IsInHotKeySetting = false;
            };
            setKeyCode(Key);
            timer.Interval = TimeSpan.FromMilliseconds(40);
            timer.Tick += (s, e) =>
            {
                if (HasWin || HasAlt || HasCtrl) 
                    return;
                rendaCount++;
                if (rendaCount > 4)
                    SimulateKeydown();
            };
        }

        public override bool OnButtonPressed()
        {
            timer.Start();
            SimulateKeydown();
            return true;
        }

        private void SimulateKeydown()
        {
            if (HasWin) WinAPI.KeyDown((int)System.Windows.Forms.Keys.LWin);
            if (HasCtrl) WinAPI.KeyDown((int)System.Windows.Forms.Keys.LControlKey);
            if (HasShift) WinAPI.KeyDown((int)System.Windows.Forms.Keys.LShiftKey);
            if (HasAlt) WinAPI.KeyDown((int)System.Windows.Forms.Keys.LMenu);
            WinAPI.KeyDown(Key);
        }

        public override bool OnButtonReleased()
        {
            timer.Stop();
            rendaCount = 0;
            if (HasWin) WinAPI.KeyUp((int)System.Windows.Forms.Keys.LWin);
            if (HasCtrl) WinAPI.KeyUp((int)System.Windows.Forms.Keys.LControlKey);
            if (HasShift) WinAPI.KeyUp((int)System.Windows.Forms.Keys.LShiftKey);
            if (HasAlt) WinAPI.KeyUp((int)System.Windows.Forms.Keys.LMenu);
            WinAPI.KeyUp(Key);
            return true;
        }

        public override void DeploySettingUI(SimpleStackPanel parent)
        {
            parent.Children.Add(HotKeySettingButton);
            parent.Children.Add(KeyName);
        }

        public void setKeyCode(int keyCode)
        {
            System.Windows.Forms.KeysConverter converter = new System.Windows.Forms.KeysConverter();
            if ((keyCode < 160 || keyCode > 165) && keyCode != 91 && keyCode != 92)
                Key = keyCode;
            string hotKey = string.Empty;
            if (HasWin) hotKey += "Win + ";
            if (HasCtrl) hotKey += "Control +";
            if (HasShift) hotKey += "Shift +";
            if (HasAlt) hotKey += "Alt + ";
            if (!HasWin && !HasCtrl && !HasShift && !HasAlt && Key == 0)
                KeyName.Text = "キー：未設定";
            else
                KeyName.Text = Key == 0 ? "キー：" + hotKey : "キー："+ hotKey + converter.ConvertToString(Key);
            KeyName.Text = KeyName.Text.TrimEnd('+');
        }

        internal void KeySettingComplete()
        {
            AcceptingKeyInput = false;
            HotKeySettingButton.Content = "ボタンを押して設定変更";
        }

        public enum SpecialKey
        {
            Pause = 0x13,

        }

        public enum NumPad
        {
            NUM0 = 0x60,
            NUM1 = 0x61,
            NUM2 = 0x62,
            NUM3 = 0x63,
            NUM4 = 0x64,
            NUM5 = 0x65,
            NUM6 = 0x66,
            NUM7 = 0x67,
            NUM8 = 0x68,
            NUM9 = 0x69,
            MULTIPLY = 0x6A,
            ADD = 0x6B,
            SUBTRACT = 0x6D,
            DIVIDE = 0x6F
        }

        public enum FunctionKey
        {
            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B,
            F13 = 0x7C,
            F14 = 0x7D,
            F15 = 0x7E,
            F16 = 0x7F,
            F17 = 0x80,
            F18 = 0x81,
            F19 = 0x82,
            F20 = 0x83,
            F21 = 0x84,
            F22 = 0x85,
            F23 = 0x86,
            F24 = 0x87,
        }

        public enum ConsumerKey
        {
            MEDIA_RECORD = 0xB2,
            MEDIA_FAST_FORWARD = 0xB3,
            MEDIA_REWIND = 0xB4,
            MEDIA_NEXT = 0xB5,
            MEDIA_PREVIOUS = 0xB6,
            MEDIA_STOP = 0xB7,
            MEDIA_PLAY_PAUSE = 0xCD,

            MEDIA_VOLUME_MUTE = 0xE2,
            MEDIA_VOLUME_UP = 0xE9,
            MEDIA_VOLUME_DOWN = 0xEA,

            BRIGHTNESS_UP = 0x006F,
            BRIGHTNESS_DOWN = 0x0070,

            EMAIL_READER = 0x18A,
            CALCULATOR = 0x192,
            EXPLORER = 0x194,

            BROWSER_HOME = 0x223,
            BROWSER_BACK = 0x224,
            BROWSER_FORWARD = 0x225,
            BROWSER_REFRESH = 0x227,
            BROWSER_BOOKMARKS = 0x22A,
        }
    }
}
