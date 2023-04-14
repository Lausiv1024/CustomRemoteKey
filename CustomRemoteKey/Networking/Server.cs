using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace CustomRemoteKey.Networking
{
    internal class Server
    {
        public const int port = 65430;

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
            Socket.Bind(Endpoint);
            Socket.Listen(10);

            Main = new Thread(new ThreadStart(Round));
            Main.Start();
            Console.WriteLine("Socket Thread started.");
        }

        void Round()
        {
            while (true)
            {
                
                SocketEvent.Reset();
                Socket.BeginAccept(new AsyncCallback(OnConnectRequest), Socket);
                SocketEvent.WaitOne();
            }
        }

        void OnConnectRequest(IAsyncResult ar)
        {
            SocketEvent.Set();
            Socket listener =(Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);//ここでObjectDisposedExceptionが出る
            StateObject obj = new StateObject();
            obj.workingSocket = handler;
            handler.BeginReceive(obj.buffer, 0, StateObject.BUFFER_SIZE, 0,
                new AsyncCallback(ReadCallback), obj);
        }

        void ReadCallback(IAsyncResult ar)
        {
            StateObject state = ar.AsyncState as StateObject;
            Socket handler = state.workingSocket;
            int readSize = handler.EndReceive(ar);
            if (readSize < 1)
            {
                return;
            }
            byte[] buffer = new byte[readSize];
            Array.Copy(state.buffer, buffer, readSize);
            string decodedText = Encoding.UTF8.GetString(buffer);
            if (decodedText == "ConTes<EOM>")
            {
                buffer = Encoding.UTF8.GetBytes("OK<EOM>");
            }else
                buffer = Encoding.UTF8.GetBytes("Test<EOM>");
            handler.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(WriteCallback), state);
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
            public Socket workingSocket;
            public const int BUFFER_SIZE = 1024;
            internal byte[] buffer = new byte[BUFFER_SIZE];
        }
    }
}
