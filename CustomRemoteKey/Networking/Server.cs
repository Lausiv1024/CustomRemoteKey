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

        public bool Closed { get; private set; }

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
            
            //try
            //{
            //    symAl = new AesCryptoServiceProvider();
            //    symAl.KeySize = 256;
            //    symAl.BlockSize = SymAlBlockSize * 8;
            //    symAl.Padding = PaddingMode.Zeros;
            //    symAl.Mode = CipherMode.CBC;
            //    symAl.GenerateIV();
            //} catch
            //{
            //    Console.WriteLine("Failed to Init Encryption");
            //}
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
                        data = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

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
            int readSize = 0;
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

            HandleData(state.buffer, readSize);
            
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

        internal void HandleData(byte[] buffer, int readSize)
        {
            byte[] bb = new byte[readSize];
            Array.Copy(buffer, bb, readSize);

            string decodedText = Encoding.UTF8.GetString(bb);
            Console.WriteLine("Binary Data : {0}", BitConverter.ToString(bb));
            if (AcceptingNewConnection && decodedText.IndexOf("NEWCON") == 0)
            {
                int ModelNameEndPos = 7;
                for (int i = 7; i < bb.Length; i++)
                {
                    if (bb[i] == ',')
                    {
                        ModelNameEndPos = i;
                        break;
                    }
                }
                byte[] deviceNameB = new byte[ModelNameEndPos - 7];
                byte[] hashVal = new byte[32];
                Array.Copy(bb, 7, deviceNameB, 0, ModelNameEndPos - 7);
                Array.Copy(bb, ModelNameEndPos + 1, hashVal, 0, hashVal.Length);

                using (var sh256 = SHA256.Create())
                {
                    var hashed = sh256.ComputeHash(Encoding.ASCII.GetBytes(currentAccessKey));
                    if (BitConverter.ToString(hashed) == BitConverter.ToString(hashVal))
                    {
                        Guid deviceId = Guid.NewGuid();
                        DeviceAdded(new DeviceAddedEventArgs(Encoding.UTF8.GetString(deviceNameB), deviceId));
                        string retData = "OK," + Environment.MachineName;

                        bb = Encoding.UTF8.GetBytes("OK<EOM>");
                    } else
                    {
                        bb = Encoding.UTF8.GetBytes("ERROR<EOM>");
                    }
                }
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
            //handler.BeginSend(bb, 0, bb.Length, 0, new AsyncCallback(WriteCallback), state);
        }

        public class StateObject
        {
            public Socket workingSocket { get; set; }
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }
    }
}
