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

namespace CustomRemoteKey.Networking
{
    internal class Server
    {
        public const int port = 60001;

        public ManualResetEvent SocketEvent = new ManualResetEvent(false);

        private IPEndPoint Endpoint;

        private Socket Socket;

        private Thread Main;

        public event EventHandler<DeviceAddedEventArgs> OnDeviceAdded;

        WiFiDirectAdvertisementPublisher publisher;

        public bool AcceptingNewConnection = false;
        public string currentAccessKey { private get; set; }


        public bool Closed { get; private set; }

        public Server()
        {
            Endpoint = new IPEndPoint(IPAddress.Any, port);
            Console.WriteLine(Dns.GetHostName());
            //IPAddress[] adrList = Dns.GetHostAddresses(Dns.GetHostName());
            //foreach (IPAddress address in adrList)
            //{
            //    Console.WriteLine(address.ToString());
            //}

            try
            {
                var hosts = Dns.GetHostEntry("MainLsv");
                foreach(var host in hosts.AddressList) {
                    Console.WriteLine(host.ToString());
                }

                publisher = new WiFiDirectAdvertisementPublisher();

            } catch(SocketException ex)
            {
                Console.WriteLine("Can't find MainLsv");
            }
        }

        internal void Init()
        {
            Closed = false;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Main = new Thread(new ThreadStart(Round));
            Main.Start();
            Console.WriteLine("Socket Thread started.");
            publisher.Start();
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

            byte[] bb = new byte[readSize];
            Array.Copy(state.buffer, bb, readSize);
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
            }
            else if (decodedText == "ConTes<EOM>")
            {
                bb = Encoding.UTF8.GetBytes("OK<EOM>");
            }else if (decodedText == "1")
            {
                MainWindow.Instance.HandleButtonPressed();
            }else if (decodedText == "0")
            {
                MainWindow.Instance.HandleButtonReleased();
            }
            else
                bb = Encoding.UTF8.GetBytes("TEST<EOM>");
            handler.BeginSend(bb, 0, bb.Length, 0, new AsyncCallback(WriteCallback), state);
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
            publisher.Stop();
        }

        protected virtual void DeviceAdded(DeviceAddedEventArgs e)
        {
            OnDeviceAdded?.Invoke(this, e);
        }

        public class StateObject
        {
            public Socket workingSocket { get; set; }
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }
    }
}
