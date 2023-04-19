using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace CustomRemoteKey.Networking
{
    internal class Server
    {
        public const int port = 60001;

        public ManualResetEvent SocketEvent = new ManualResetEvent(false);

        private IPEndPoint Endpoint;

        private Socket Socket;

        private Thread Main;

        public bool Closed { get; private set; }

        public Server()
        {
            Endpoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void Init()
        {
            Closed = false;
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Main = new Thread(new ThreadStart(Round));
            Main.Start();
            Console.WriteLine("Socket Thread started.");
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
            Socket handler = listener.EndAccept(ar);//ここでObjectDisposedExceptionが出る
            StateObject state = new StateObject();
            state.workingSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallback), state);
        }

        void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workingSocket;
            int readSize = handler.EndReceive(ar);

            if (readSize < 1)
            {
                return;
            }

            byte[] bb = new byte[readSize];
            Array.Copy(state.buffer, bb, readSize);
            string decodedText = Encoding.UTF8.GetString(bb);
            if (decodedText == "ConTes<EOM>")
            {
                bb = Encoding.UTF8.GetBytes("OK<EOM>");
            }else
                bb = Encoding.UTF8.GetBytes("Test<EOM>");
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
        }

        public class StateObject
        {
            public Socket workingSocket { get; set; }
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }
    }
}
