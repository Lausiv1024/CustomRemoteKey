
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace Phone
{
    public partial class MainPage : ContentPage
    {
        bool[,] hapticCooldown;
        private const int ProfileCount = 6;
        private const int ButtonCountX = 4;
        private const int ButtonCountY = 5;

        private int ProfileMode = 0;//そのうちデスクトップ側が全部管理するようになるんだ

        TcpClient Client = new TcpClient();

        bool IsConnected = false;

        int tickCount;

        public static MainPage Instance { get; private set; }

        Client.Session? CurrentSession = null;

        Timer timer;

        
        public MainPage()
        {
            InitializeComponent();

            //AllParent.RowDefinitions[0].Height = DeviceDisplay.Current.MainDisplayInfo.Width /
            //    (ProfileCount * 3);

            for (int i = 0; i < ProfileCount; i++)
            {
                ProfileSelect.ColumnDefinitions.Add(new ColumnDefinition());
                Button but = new Button();
                but.Text = (i + 1).ToString();
                but.Margin = new Thickness(4);
                but.Pressed += OnProfileButtonPressed;
                Grid.SetColumn(but, i);
                ProfileSelect.Children.Add(but);
            }

            hapticCooldown = new bool[ButtonCountX,ButtonCountY];

            for (int i = 0; i < ButtonCountX; i++) ControlButtonDeck.AddColumnDefinition(new ColumnDefinition());
            for (int i = 0; i < ButtonCountY + 1; i++) ControlButtonDeck.AddRowDefinition(new RowDefinition());
            for (int i = 0; i < ButtonCountX * ButtonCountY; i++)
            {
                var button = new Button();

                button.Margin = new Thickness(6);
                button.Released += ControlButtonUp;
                button.Pressed += ControlButtonDown;
                Grid.SetColumn(button, i % ButtonCountX);
                Grid.SetRow(button, i / ButtonCountX);
                ControlButtonDeck.Children.Add(button);
            }

            Slider label = new Slider();
            label.Margin = new Thickness(10);
            
            Grid.SetRow(label, ButtonCountY);
            Grid.SetColumnSpan(label, ButtonCountX);
            ControlButtonDeck.Children.Add(label);
            Client = new TcpClient();
            Client.SendTimeout = 2000;
            Client.ReceiveTimeout = 2000;
            timer = new Timer((state) =>
            {
                if (IsConnected)
                {
                    Dispatcher.Dispatch(() => Title = "Controls - Connected");
                    tickCount++;
                    try
                    {
                        if (tickCount == 10)
                        {
                            if (!Client.Connected)
                            {
                                IsConnected = false;
                                return;
                            }
                            sendEncryptionData(Encoding.ASCII.GetBytes("ConTes<EOM>"), CurrentSession.AESKey, CurrentSession.AESIV);
                            tickCount = 0;
                        }
                    } catch
                    {
                        Debug.WriteLine("Error");
                    }
                    return;
                }
                //try
                //{
                //    Client.Connect(host, PORT);
                //    IsConnected = true;
                //} catch (Exception ex)
                //{
                //    if (ex.GetType() == typeof(SocketException))
                //    {
                //        Dispatcher.Dispatch(() =>
                //        {
                //            Title = "Controls - Not Connected";
                //        });
                //    }
                //}
            }, null, new TimeSpan(0), TimeSpan.FromMilliseconds(1000));
            Instance = this;
        }

        private void OnConnected(object sender, EventArgs e)
        {

        }

        public async Task<TcpClient> TryConnectAsync(string ipAddr)
        {
            TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(IpFromString(ipAddr), CRKConstants.TCP_PORT);
            } catch(Exception ex)
            {
                client.Close();
                Debug.WriteLine(ex.Message);
                return null;
            }
            if (IsConnected) client.Close();
            return client;
        }

        private async void disconnect()
        {
            if (Client is null)
                return;
            if (!Client.Connected || CurrentSession is null)
                return;
            string received;
            if (CurrentSession.connectionStage ==0)
            {
                SendData(Encoding.ASCII.GetBytes("D"));

                received = Encoding.ASCII.GetString(await receiveDataAsync());
            } else
            {
                sendEncryptionData(Encoding.ASCII.GetBytes("D"), CurrentSession.AESKey, CurrentSession.AESIV);
                received = await receiveAndDecryptDataAsync(CurrentSession.AESKey, CurrentSession.AESIV);
            }

            if (received == "DSC_OK")
            {
                Client.Close();
                CurrentSession = null;
            }
        }
        
        /// <summary>
        /// 接続要求を送信します
        /// </summary>
        /// <param name="ipAddr">接続先IPアドレス</param>
        /// <param name="encryptedCommonKey">RSAにより暗号化されたAES共通鍵</param>
        /// <param name="aesKey">AES鍵</param>
        /// <param name="aesIV">AES初期化ベクトル</param>
        /// <returns></returns>
        public async Task<bool> ConnectTo(string[] ipAddr, byte[] encryptedCommonKey, byte[] aesKey, byte[] aesIV)
        {
            if (ipAddr == null) return false;
            if (ipAddr.Length == 0) return false;

            var tryConnectTasks = new List<Task<TcpClient>>();
            foreach (string ip in ipAddr)
            {
                tryConnectTasks.Add(TryConnectAsync(ip));
            }
            
            await Task.WhenAny(tryConnectTasks);
            await Task.Delay(500);

            foreach (var task in tryConnectTasks)
            {
                if (task.IsCompleted)
                {
                    var client = task.Result;
                    if (client == null) continue;
                    if (!IsConnected)
                    {
                        Client = client;
                        IsConnected = true;
                    }
                }
            }

            //デバッグではここの処理が無視されていた．処理がスキップされるなんてことある？
            //Stage 0-0-C 接続要求And鍵交換
            var newConnection = Encoding.UTF8.GetBytes("NEWCON");
            var sendData = new byte[newConnection.Length + encryptedCommonKey.Length];
            Array.Copy(newConnection, sendData, newConnection.Length);
            Array.Copy(encryptedCommonKey, 0, sendData, newConnection.Length, encryptedCommonKey.Length);
            //Client.Client.Send(sendData);
            SendData(sendData);
            byte[] buffer = new byte[CRKConstants.BUFFER_SIZE];
            var received = await Client.Client.ReceiveAsync(buffer, SocketFlags.None);
            //Stage 0-1-C デバイスの名前の送信と鍵データの登録
            if (Encoding.ASCII.GetString(buffer, 0, received) == "OK<EOM>")
            {
                string deviceNameCmd = "DENAME" + DeviceInfo.Current.Model + "<EOM>";
                sendEncryptionData(Encoding.UTF8.GetBytes(deviceNameCmd), aesKey, aesIV);
                var result = await receiveAndDecryptDataAsync(aesKey, aesIV);
                if (result != "OK<EOM>") return false;
                IsConnected = true;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopToRootAsync();
                });
                CurrentSession = new();
                CurrentSession.AESKey = aesKey;
                CurrentSession.AESIV = aesIV;
                CurrentSession.connectionStage++;
            } else
            {
                DispAlertFromOtherThread("ERROR", "クライアントに正常に接続できませんでした。\nもう一度やり直してください。", "OK");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 先頭にデータサイズを付加し送信します．
        /// </summary>
        /// <param name="data">送信するデータのバイト列</param>
        private void SendData(byte[] data)
        {
            if (data.Length > byte.MaxValue) return;
            byte dataSize = (byte)data.Length;
            byte[] dataSizeAdded = new byte[dataSize + 1];
            Array.Copy(data, 0, dataSizeAdded, 1, dataSize);
            dataSizeAdded[0] = dataSize;
            Client.Client.Send(dataSizeAdded);
        }

        /// <summary>
        /// データを暗号化してサーバーにデータを送信します．
        /// </summary>
        /// <param name="data">送信するデータのバイト列</param>
        /// <param name="key">AES暗号鍵</param>
        /// <param name="iv">AES初期化ベクトル</param>
        private void sendEncryptionData(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = CRKConstants.AES_BLOCKSIZE;
                aes.KeySize = CRKConstants.AES_KEYSIZE;
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                SendData(encryptor.TransformFinalBlock(data, 0, data.Length));
            }
        }

        private async Task<string> receiveAndDecryptDataAsync(byte[] key,byte[] iv)
        {
            byte[] receivedData = await receiveDataAsync();
            
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = CRKConstants.AES_BLOCKSIZE;
                aes.KeySize = CRKConstants.AES_KEYSIZE;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor();

                return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(receivedData, 0, receivedData.Length));

            }
        }

        private async Task<byte[]> receiveDataAsync()
        {
            if (!Client.Connected || CurrentSession is null)
                return null;
            byte[] buffer = new byte[CRKConstants.BUFFER_SIZE];
            var received = await Client.Client.ReceiveAsync(buffer, SocketFlags.None);
            byte[] receivedData = new byte[received];
            Array.Copy(buffer, receivedData, receivedData.Length);
            return receivedData;
        }

        IPAddress IpFromString(string ip)
        {
            if ( IPAddress.TryParse(ip, out IPAddress ipAddress))
            {
                return ipAddress;
            }
            return null;
        }

        private static void DispAlertFromOtherThread(string title, string message, string cancel)
        {
            MainThread.BeginInvokeOnMainThread(async() =>
            {
                await Instance.DisplayAlert(title, message, cancel);
            });
        }
        private void OnDisconnected(object sender, EventArgs e)
        {

        }

        private void OnReceiveData(object sender, EventArgs e)
        {

        }

        private Button GetControlButton(int column, int row)
        {   
            if (row >= ButtonCountY || column >= ButtonCountX) return null;
            return ControlButtonDeck.Children.Cast<Button>()
                .Where(b => Grid.GetRow(b) == row && Grid.GetColumn(b) == column).FirstOrDefault();
        }

        private void OnProfileButtonPressed(object sender, EventArgs e)
        {
            var button = sender as Button;
            ProfileMode = Grid.GetColumn(button);
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            GetControlButton(2,3).Text = ProfileMode.ToString();
        }

        private async void ControlButtonDown(object sender, EventArgs e)
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            Button button = (Button)sender;
            int buttonX = Grid.GetColumn(button); int buttonY = Grid.GetRow(button);
            if (IsConnected)
            {
                sendEncryptionData(Encoding.ASCII.GetBytes($"P {buttonX} {buttonY}"), CurrentSession.AESKey, CurrentSession.AESIV);
            }
            hapticCooldown[buttonX, buttonY] = true;
            await Task.Delay(100);
            hapticCooldown[buttonX, buttonY] = false;
            tickCount = 0;
        }

        private void ControlButtonUp(object sender, EventArgs e)
        {
            Button button  = (Button)sender;
            int buttonX = Grid.GetColumn(button); int buttonY = Grid.GetRow(button);
            if (IsConnected)
            {
                sendEncryptionData(Encoding.ASCII.GetBytes($"R {buttonX} {buttonY}"), CurrentSession.AESKey, CurrentSession.AESIV);
            }
            if (!hapticCooldown[buttonX, buttonY])
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            tickCount = 0;
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
#if ANDROID
            DeviceDisplay.Current.KeepScreenOn = false;
            //if (Client.Connected)
            //    Client.Close();
#endif
            Debug.WriteLine("Disappering");
        }

        public void OnAppPause()
        {
            Debug.WriteLine("OnAppPause");
            //disconnect();
        }

        public void OnAppResume()
        {
            Debug.WriteLine("OnAppResume");
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            DeviceDisplay.Current.KeepScreenOn = true;
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingPage());
        }

        public enum OPCodes
        {
            AUTHPRI = 0,
            AUTHPUB = 1,
            KEYINPUT = 2,
        }
    }
}