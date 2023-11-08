using CustomRemoteKey.Event.Args;
using QRCoder;
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
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CustomRemoteKey
{
    /// <summary>
    /// AddDevice.xaml の相互作用ロジック
    /// </summary>
    public partial class AddDevice : Window
    {
        public int code = 0;
        private DeviceProperty property;
        private string debugCurrentQRData;
        public AddDevice()
        {
            InitializeComponent();
            //code = new Random().Next(100000, 999999);
            code = 114514;
            MainWindow.Instance.MainServer.OnDeviceAdded += DeviceAdded;
            property = new DeviceProperty();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ConnectionCode.Text = code.ToString();
            MainWindow.Instance.MainServer.currentAccessKey = code.ToString();
            var qrGen = new QRCodeGenerator();
            var ipList = Util.GetIpAddr();

            Util.CreateRSAKeys(out string publicKey, out string privateKey);
            MainWindow.Instance.MainServer.setPublicEncryptionData(publicKey, privateKey);
            Console.WriteLine("PubKey : {0}\n PriKey : {1}", publicKey, privateKey);
            if (ipList.Length != 0)
            {
                string ipStr = "";
                foreach (var ip in ipList)
                {
                    ipStr += ip + ",";
                }
                ipStr += "AccessKey=" + publicKey;
                debugCurrentQRData = ipStr;
                var data = qrGen.CreateQrCode(ipStr, QRCodeGenerator.ECCLevel.L);
                var a = new BitmapByteQRCode(data);
                QRImg.Source = Util.ToBitmapImage(a.GetGraphic(10));
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Instance.MainServer.AcceptingNewConnection = false;
            MainWindow.Instance.MainServer.currentAccessKey = "";
        }

        private void DeviceAdded(object sender, DeviceAddedEventArgs e)
        {

            Dispatcher.Invoke(() => this.Close());
        }

        private void QRImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(debugCurrentQRData);
        }
    }
}
