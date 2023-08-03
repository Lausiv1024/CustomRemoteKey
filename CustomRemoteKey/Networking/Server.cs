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

        private void Round2()
        {
            
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 13500);
                listener.Start();
                byte[] buffer = new byte[256];
                string data = null;
                while (!Closed)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(buffer, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }
                }
            } catch(SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            } finally
            {
                listener.Stop();
            }
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
            int readSize;
            try
            {
                readSize = handler.EndReceive(ar);
            } catch {
                return;
            }
            if (readSize < 1)
            {
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

        internal void HandleData(byte[] buffer, int readSize, StateObject state)
        {
            byte[] bb = new byte[readSize];
            bool isSuccess = false;
            Array.Copy(buffer, bb, readSize);
            var handle = state.workingSocket.Handle;
            Console.WriteLine("Handle : {0}", handle);
            string decodedText = Encoding.UTF8.GetString(bb);
            Console.WriteLine("Binary Data : {0}", BitConverter.ToString(bb));
            if (AcceptingNewConnection && decodedText.IndexOf(NEWCONNECTIONCODE) == 0)
            {
                Console.WriteLine("NewConnection Requested");
                byte[] encryptedData = new byte[readSize - Encoding.ASCII.GetByteCount(NEWCONNECTIONCODE)];
                Array.Copy(bb, 6, encryptedData, 0, encryptedData.Length);
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(RSAPrivateKey);
                byte[] aesKeyData;
                try
                {
                    aesKeyData = provider.Decrypt(encryptedData, false);
                    Console.WriteLine("keySize : {0}", aesKeyData.Length);
                    bb = Encoding.ASCII.GetBytes("OK<EOM>");
                    isSuccess = true;
                    Console.WriteLine(BitConverter.ToString(aesKeyData));
                } catch (CryptographicException)
                {
                    bb = Encoding.ASCII.GetBytes("ERROR<EOM>");
                    Console.WriteLine("Failed");
                }
                if (isSuccess)
                    OnDeviceAdded?.Invoke(this, new DeviceAddedEventArgs("Test", Guid.NewGuid()));
            } else if (decodedText == "ConTes<EOM>")
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
            state.workingSocket.BeginSend(bb, 0, bb.Length, 0, new AsyncCallback(WriteCallback), state);
        }

        public class StateObject
        {
            public Socket workingSocket { get; set; }
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }
    }
}
