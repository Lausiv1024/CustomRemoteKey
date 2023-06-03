using CustomRemoteKey.Behaviours;
using CustomRemoteKey.Native;
using CustomRemoteKey.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomRemoteKey
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        internal Server MainServer { get; private set; }

        Server a = null;

        private const int ButtonCountX = 4;
        private const int ButtonCountY = 5;
        Style ButtonDefault;

        int currentProfileMode = 0;
        int selectedButtonX = -1, selectedButtonY = -1;

        bool NotSelected => selectedButtonX == -1 && selectedButtonY == -1;
        internal bool IsInHotKeySetting = false;

        //現在設定中のデバイス
        int currentDevice = -1;

        public List<DeviceProperty> devices = new List<DeviceProperty>();

        KeyboardHook hook;

        public MainWindow()
        {
            InitializeComponent();
            
            this.Hide();
#if DEBUG
            this.Show();
#endif
            MainServer = new Server();
            try
            {
                MainServer.Init();
            } catch
            {
                Console.WriteLine("Failed to launch server");
            }

            for (int i = 0; i < ButtonCountX; i++)
                Buttons.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < ButtonCountY; i++)
                Buttons.RowDefinitions.Add(new RowDefinition());
            int count = 0;
            for (int x = 0; x < ButtonCountX; x++)
            {
                for (int y = 0; y < ButtonCountY; y++)
                {
                    var but = new Button();
                    Grid.SetRow(but, y);
                    Grid.SetColumn(but, x);
                    but.HorizontalAlignment = HorizontalAlignment.Stretch;
                    but.VerticalAlignment = VerticalAlignment.Stretch;
                    but.Margin = new Thickness(10);
                    
                    but.Click += (s, e) =>
                    {
                        Button button =(Button) s;
                        var a = Buttons.Children.Cast<Button>();
                        foreach (var b in a)
                        {
                            b.Style = ButtonDefault;
                        }
                        
                        button.Style =(Style) FindResource("AccentButtonStyle");
                        Console.WriteLine("Button Clicked Row : {0} Column : {1}", Grid.GetRow(button), Grid.GetColumn(button));
                        selectedButtonX = Grid.GetColumn(button);
                        selectedButtonY = Grid.GetRow(button);
                        var d = devices[currentDevice];
                        
                        ProfName.Text = d.ButtonName[currentProfileMode, selectedButtonX + selectedButtonY * 4];
                        setUISelectionFromBehaviour(d.Behaviours[currentProfileMode, selectedButtonX + selectedButtonY * 4]);
                    };
                    Buttons.Children.Add(but);
                    
                    ButtonDefault = but.Style;
                    count++;
                    Activated += (s, e) =>
                    {
                        if (!hook.IsHooking) hook.Hook();
                    };
                    Deactivated +=(s, e) =>{
                        if (hook.IsHooking) hook.Unhook();
                    };
                }
            }
            devices.Add(new DeviceProperty() { Id = Guid.NewGuid(), Name = "Sample"});

            SelectDevice.ContextMenu = new ContextMenu();
            SelectDevice.Click += (s, e) => SelectDevice.ContextMenu.IsOpen = true;
            
            var item = new MenuItem();
            item.Header = "デバイスの追加";
            item.Click += (s, e) =>
            {
                MainServer.AcceptingNewConnection = true;
                var dialog = new AddDevice();
                dialog.ShowDialog();
            };
            SelectDevice.ContextMenu.Items.Add(item);
            currentDevice = 0;
            foreach (var device in devices)
            {
                AddNewDevice(device);
            }
            if (currentDevice >= 0)
            {
                SelectDevice.Content = devices[currentDevice].Name;
                LoadProfile();
            }

            MainServer.OnDeviceAdded += (s, e) =>
            {
                var newDevice = new DeviceProperty();
                newDevice.Id = e.DeviceId;
                newDevice.Name = e.DeviceName;
                devices.Add(newDevice);
                Dispatcher.Invoke(() => AddNewDevice(newDevice));
            };

            Instance = this;

            hook = new KeyboardHook();
            hook.KeyDownEvent += Hook_KeyDownEvent;
            hook.KeyUpEvent += (s, e) =>
            {
                if (NotSelected) return;
                var behaviour1 = devices[currentDevice].Behaviours[currentProfileMode, selectedButtonX + selectedButtonY * 4];
                if (behaviour1 != null && behaviour1 is InputHotKey)
                {
                    var hotkey = behaviour1 as InputHotKey;
                    hotkey.KeySettingComplete();
                }

                IsInHotKeySetting = false;
            };
            hook.Hook();
        }

        private void Hook_KeyDownEvent(object sender, KeyboardHook.KeyEventArg e)
        {
            Console.WriteLine("KeyCode : {0}", e.KeyCode);
            e.TrashInput = IsInHotKeySetting;
            if (IsInHotKeySetting && !NotSelected)
            {
                var behaviour = devices[currentDevice].Behaviours[currentProfileMode, selectedButtonX + selectedButtonY * 4];
                if (behaviour != null && behaviour is InputHotKey)
                {
                    var hotkey = behaviour as InputHotKey;
                    if (e.KeyCode == 160 || e.KeyCode == 161)
                        hotkey.HasShift = true;
                    else if (e.KeyCode == 162 || e.KeyCode == 163)
                        hotkey.HasCtrl = true;
                    else if (e.KeyCode == 164 || e.KeyCode == 165)
                        hotkey.HasAlt = true;
                    else if (e.KeyCode == 91 || e.KeyCode == 92)
                        hotkey.HasWin = true;
                    hotkey.setKeyCode(e.KeyCode);
                }
            }
        }

        private void AddNewDevice(DeviceProperty device)
        {
            var d = new MenuItem();
            d.Header = device.Name;
            d.Click += (s, e) =>
            {
                var item12 = (MenuItem)s;
                SelectDevice.Content = item12.Header;
            };
            SelectDevice.ContextMenu.Items.Add(d);
        }

        private void LoadProfile()
        {
            for (int i = 0; i < 20; i++)
            {
                GetControlButton(i % 4, i / 4).Content =
                 devices[currentDevice].ButtonName[currentProfileMode, i];
            }
            if (selectedButtonX < 0 || selectedButtonY < 0) return;
            ProfName.Text = devices[currentDevice].ButtonName[currentProfileMode, selectedButtonX + selectedButtonY * 4];
        }

        private Button GetControlButton(int column, int row)
        {
            if (row >= ButtonCountY || column >= ButtonCountX) return null;
            return Buttons.Children.Cast<Button>()
                .Where(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == column).FirstOrDefault();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
#if DEBUG
            Close_Click(null, null);
#endif
        }

        private void OnProfileButtonClicked(object sender, RoutedEventArgs e)
        {
            Button but  = (Button) sender;
            int profileIndex = Grid.GetColumn(but);
            currentProfileMode = profileIndex;
            Console.WriteLine("Profile {0}", profileIndex);
            LoadProfile();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            await Task.Delay(3000);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MainServer.Close();
            Thread.Sleep(80);
            hook.Unhook();
            Application.Current.Shutdown();
        }

        private void ProfName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedButtonX < 0 || selectedButtonY < 0) return;
            var button = GetControlButton(selectedButtonX, selectedButtonY);
            button.Content = ((TextBox) sender).Text;
            devices[currentDevice].ButtonName[currentProfileMode, selectedButtonX + selectedButtonY * 4] = (string)button.Content;
        }

        private void Profiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotSelected) return;
            var behaviour = GetBehaviour(Profiles.SelectedIndex);
            devices[currentDevice].Behaviours[currentProfileMode, selectedButtonX + selectedButtonY * 4] = behaviour;
            updateSettingUI(behaviour);
        }

        private BehaviourBase GetBehaviour(int index)
        {
            var behaviour = devices[currentDevice].Behaviours[currentProfileMode, selectedButtonX + selectedButtonY * 4];
            switch (index)
            {
                case 1://InputHotKey
                    return behaviour != null && (behaviour is InputHotKey) ? behaviour : new InputHotKey();
                case 2:
                    return behaviour != null && behaviour is PlaySound ? behaviour : new PlaySound();
            }
            return null;
        }

        private void setUISelectionFromBehaviour(BehaviourBase behaviour)
        {
            if (behaviour is null)
                Profiles.SelectedIndex = 0;
            else if (behaviour is InputHotKey)
                Profiles.SelectedIndex = 1;
            else if (behaviour is PlaySound)
                Profiles.SelectedIndex = 2;
            updateSettingUI(behaviour);
        }

        private void updateSettingUI(BehaviourBase behaviour)
        {
            BehaviourContext.Children.Clear();
            behaviour?.DeploySettingUI(BehaviourContext);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }

        public void HandleButtonPressed()
        {
            var firstBehaviour = devices[currentDevice].Behaviours[currentProfileMode, 0];
            if (firstBehaviour != null && firstBehaviour is InputHotKey)
            {
                firstBehaviour.OnButtonPressed();
            }
        }
        public void HandleButtonReleased()
        {
            var firstBehaviour = devices[currentDevice].Behaviours[currentProfileMode, 0];
            if (firstBehaviour != null && firstBehaviour is InputHotKey)
            {
                firstBehaviour.OnButtonReleased();
            }
        }
    }
}