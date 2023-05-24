using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace CustomRemoteKey.Behaviours
{
    internal class InputHotKey : BehaviourBase
    {
        public System.Windows.Forms.Keys Key { get; set; }
        private CheckBox hasWinKey { get; set; }
        private CheckBox hasShiftKey { get; set; }
        private CheckBox hasCtrlKey { get; set; }
        private CheckBox hasAltKey { get; set; }
        
        public bool HasWinKey;
        public bool HasShift;
        public bool HasCtrl;
        public bool HasAlt;
        private bool AcceptingKeyInput = false;
        public InputHotKey() : base("CustomHotKey", "ホットキー")
        {
            Key = System.Windows.Forms.Keys.None;
            hasWinKey = new CheckBox();
            hasShiftKey = new CheckBox();
            hasCtrlKey = new CheckBox();
            hasAltKey = new CheckBox();
            
            hasWinKey.Content = "Windowsキー";
            hasWinKey.Click += (s, e) => HasWinKey = (bool)hasWinKey.IsChecked;
            hasWinKey.Content = "Ctrlキー";
            hasCtrlKey.Click += (s, e) => HasCtrl = (bool)hasCtrlKey.IsChecked;
            hasShiftKey.Content = "Shiftキー";
            hasShiftKey.Click += (s, e) => HasShift = (bool)hasShiftKey.IsChecked;
            hasAltKey.Content = "Altキー";
            hasAltKey.Click += (s, e) => HasAlt = (bool)hasAltKey.IsChecked;
        }

        public override bool OnButtonPressed()
        {
            if (HasWinKey) WinAPI.KeyDown(System.Windows.Forms.Keys.LWin);
            if (HasCtrl) WinAPI.KeyDown(System.Windows.Forms.Keys.LControlKey);
            if (HasShift) WinAPI.KeyDown(System.Windows.Forms.Keys.LShiftKey); 
            if (HasAlt) WinAPI.KeyDown(System.Windows.Forms.Keys.LMenu);
            WinAPI.KeyDown(Key);
            return true;
        }

        public override bool OnButtonReleased()
        {
            if (HasWinKey) WinAPI.KeyUp(System.Windows.Forms.Keys.LWin);
            if (HasCtrl) WinAPI.KeyUp(System.Windows.Forms.Keys.LControlKey);
            if (HasShift) WinAPI.KeyUp(System.Windows.Forms.Keys.LShiftKey);
            if (HasAlt) WinAPI.KeyUp(System.Windows.Forms.Keys.LMenu);
            WinAPI.KeyUp(Key);
            return true;
        }

        public override void DeploySettingUI(SimpleStackPanel parent)
        {
            
            var KeyButton = new Button();
            if (Key == System.Windows.Forms.Keys.None)
                KeyButton.Content = "ボタンを押してキー設定";
            else KeyButton.Content = $"{Key.ToString()} : ボタン押して再設定";
            KeyButton.Click += (s, e) =>
            {
                AcceptingKeyInput = !AcceptingKeyInput;
                if (AcceptingKeyInput)
                {
                    if (Key == System.Windows.Forms.Keys.None)
                        KeyButton.Content = "ボタンを押してキー設定";
                    else KeyButton.Content = $"{Key.ToString()} : ボタン押して再設定";
                } else
                {
                    KeyButton.Content = "キーを押す";
                }
            };
            
            parent.Children.Add(KeyButton);
            parent.Children.Add(hasWinKey);
            parent.Children.Add(hasWinKey);
            parent.Children.Add(hasShiftKey);
            parent.Children.Add(hasAltKey);
        }
    }
}
