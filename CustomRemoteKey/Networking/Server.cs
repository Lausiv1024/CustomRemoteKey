using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using Windows.Devices.Usb;
using CustomRemoteKey.Event.Args;
using Windows.Networking;
using Windows.Devices.WiFiDirect;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;
using Windows.Networking.Sockets;
using System.IO;
using Windows.ApplicationModel.VoiceCommands;
using System.Windows.Media.Effects;
using Windows.Security.Cryptography.Certificates;

namespace CustomRemoteKey.Networking
{
    internal class Server
    {
        public const int port = 60001;

        public ManualResetEvent SocketEvent = new ManualResetEvent(false);

        private IPEndPoint Endpoint;

        private Socket Socket;

        private Thread Main;

        //DataReader dataReader;
        //DataWriter dataWriter;
        //StreamSocket streamSocket;

        public event EventHandler<DeviceAddedEventArgs> OnDeviceAdded;
        public bool AcceptingNewConnection = false;
        public string currentAccessKey { private get; set; }

        private SymmetricAlgorithm symAl;
        private static Random random = new Random();
        internal const int SymAlBlockSize = 16;
        private TcpListener listener;
        private string RSAPubKey;
        private string RSAPrivateKey;
        private byte[] CurrentAESKey;
        private byte[] currentCurrentAESIV;
        private Dictionary<string, Session> Sessions = new Dictionary<string, Session>();
        

        public bool Closed { get; private set; }

        public void setPublicEncryptionData(string RSAPubKey, string RSAPrivateKey)
        {
            this.RSAPubKey = RSAPubKey;
            this.RSAPrivateKey = RSAPrivateKey;
        }

        public Server()
        {
            Endpoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine(Dns.GetHostName());
        }

        internal void Init()
        {
            Closed = false;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Main = new Thread(new ThreadStart(Round));
            Main.Start();
            Console.WriteLine("Socket Thread started.");
        }

        internal void PrepareNewConnection()
        {

        }

        private Stream GetNetworkStream()
        {
            return new NetworkStream(Socket);
        }

        void Round()
        {
            try
            {
                Socket.Bind(Endpoint);
                Socket.Listen(10);
                while (true)
                {
                    SocketEvent.Reset();
                    Thread.Sleep(10);
                    Socket.BeginAccept(new AsyncCallback(OnConnectRequest), Socket);
                    SocketEvent.WaitOne();
                }
            } catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        void OnConnectRequest(IAsyncResult ar)
        {
            Thread.Sleep(10);
            SocketEvent.Set();
            if (Closed) return;

            Socket listener =(Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workingSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallback), state);
        }

        void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workingSocket;
            var addr = state.workingSocket.RemoteEndPoint.ToString();
            int readSize;
            try
            {
                readSize = handler.EndReceive(ar);
            } catch {
                return;
            }
            if (readSize < 1)
            {
                Sessions.Remove(addr);
                return;
            }

            HandleData(state.buffer, readSize, state);
        }

        void WriteCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workingSocket;
            handler.EndSend(ar);
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 
                0, new AsyncCallback(ReadCallback), state);
        }

        public void Close()
        {
            Closed = true;
            Socket.Close();
        }

        protected virtual void DeviceAdded(DeviceAddedEventArgs e)
        {
            OnDeviceAdded?.Invoke(this, e);
        }

        readonly string NEWCONNECTIONCODE = "NEWCON";

        const int KEYSIZE = 32, IVSIZE = 16;
        internal void HandleData(byte[] buffer, int readSize, StateObject state)
        {
            byte[] bb = new byte[readSize];
            bool isSuccess = false;
            Array.Copy(buffer, bb, readSize);
            var handle = state.workingSocket.Handle;
            string addr = state.workingSocket.AddressFamily.ToString();
            Console.WriteLine("Handle : {0}", handle);
            string decodedText = string.Empty;
            bool encryptedMode = Sessions.TryGetValue(state.workingSocket.RemoteEndPoint.ToString(), out Session session);
            Console.WriteLine(BitConverter.ToString(bb));
            if (encryptedMode && session.ConnectionStage != 0)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = session.AESKey; aes.IV = session.AESIV; aes.BlockSize = 128; aes.KeySize = 256;//aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(bb))
                    {
                        using (CryptoStream csDecrypt =  new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(csDecrypt))
                            {
                                decodedText = reader.ReadToEnd();
                            }
                        }
                    }
                }
            } else
            {
                decodedText = Encoding.UTF8.GetString(bb);
            }
            Console.WriteLine("Binary Data : {0}", BitConverter.ToString(bb));
            if (AcceptingNewConnection && decodedText.IndexOf(NEWCONNECTIONCODE) == 0)
            {
                Console.WriteLine("NewConnection Requested");
                byte[] encryptedData = new byte[readSize - Encoding.ASCII.GetByteCount(NEWCONNECTIONCODE)];
                Array.Copy(bb, 6, encryptedData, 0, encryptedData.Length);
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(RSAPrivateKey);

                var currentSession = new Session();
                Sessions.Add(state.workingSocket.RemoteEndPoint.ToString(), currentSession);
                
                byte[] aesKeyData;
                try
                {
                    aesKeyData = provider.Decrypt(encryptedData, false);

                    Console.WriteLine("keySize : {0}", aesKeyData.Length);
                    byte[] key = new byte[KEYSIZE], iv = new byte[IVSIZE];

                    Array.Copy(aesKeyData, key, KEYSIZE);
                    Array.Copy(aesKeyData, key.Length, iv, 0, IVSIZE);

                    currentSession.AESKey = key;
                    currentSession.AESIV = iv;

                    bb = Encoding.ASCII.GetBytes("OK<EOM>");
                    isSuccess = true;

                    Console.WriteLine(BitConverter.ToString(aesKeyData));
                } catch (CryptographicException)
                {
                    bb = Encoding.ASCII.GetBytes("ERROR<EOM>");
                    Console.WriteLine("Failed");
                }
                if (isSuccess)
                {
                    currentSession.ConnectionStage++;
                }
            }
            else if (decodedText.IndexOf("DENAME") == 0)
            {
                Session current = Sessions[addr];
                string deviceName = decodedText.Replace("DENAME", "").Replace("<EOM>", "");
                Guid guid = Guid.NewGuid();
                OnDeviceAdded?.Invoke(this, new DeviceAddedEventArgs(deviceName, guid));
                current.DeviceGUID = guid;
                bb = Encoding.ASCII.GetBytes("OK<EOM>");
            }
            else if (decodedText == "ConTes<EOM>")
            {
                bb = Encoding.UTF8.GetBytes("OK<EOM>");
            } else if (decodedText == "1")
            {
                MainWindow.Instance.HandleButtonPressed();
            } else if (decodedText == "0")
            {
                MainWindow.Instance.HandleButtonReleased();
            } else
                bb = Encoding.UTF8.GetBytes("TEST<EOM>");
            
            if (session != null && session.ConnectionStage != 0)
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = session.AESKey; aes.IV = session.AESIV;aes.KeySize = 256; aes.BlockSize = 128; //aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(csEncrypt))
                            {
                                sw.Write(bb);
                            }
                            bb = ms.ToArray();
                        }
                    }
                }
            }
            state.workingSocket.BeginSend(bb, 0, bb.Length, 0, new AsyncCallback(WriteCallback), state);
        }

        public class StateObject
        {
            public Socket workingSocket { get; set; }
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }

        internal class Session
        {
            internal int ConnectionStage = 0;
            internal byte[] AESKey;
            internal byte[] AESIV;
            internal Guid DeviceGUID;
            internal string DeviceName;
        }
    }
}
