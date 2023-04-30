﻿using CustomRemoteKey.Networking;
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
        Server MainServer;

        Server a = null;

        private const int ButtonCountX = 4;
        private const int ButtonCountY = 5;

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
                            b.Style = (Style) FindResource("ButtonBackground");
                        }
                        
                        button.Style =(Style) FindResource("AccentButtonStyle");
                        Console.WriteLine("Button Clicked Row : {0} Column : {1}", Grid.GetRow(button), Grid.GetColumn(button));
                    };
                    Buttons.Children.Add(but);
                    count++;
                }
            }
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

        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            await Task.Delay(3000);
            WinAPI.SendInputKeyPress(System.Windows.Forms.Keys.Enter);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MainServer.Close();
            Thread.Sleep(80);
            Application.Current.Shutdown();
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
        }
    }
}
