using CustomRemoteKey.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();

            MainServer = new Server();

            MainServer.Init();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!MainServer.Closed) e.Cancel = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            MainServer.Close();
        }
    }
}
