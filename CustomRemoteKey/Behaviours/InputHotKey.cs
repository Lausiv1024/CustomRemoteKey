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
                    HotKeySettingButton.Content = "変更中";
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
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += (s, e) =>
            {
                rendaCount++;
                if (rendaCount > 5)
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
    }
}
