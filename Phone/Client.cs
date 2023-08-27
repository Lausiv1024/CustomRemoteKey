using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Phone
{
    internal class Client
    {
        string Address;
        string AccessKey;
        bool connected;
        int connectionStage = 0;
        private const int PORT = 60001;

        TcpClient client;
        
        public Client()
        {
            new Timer((state) =>
            {
                if (!connected) return;
            }, null, new TimeSpan(0), TimeSpan.FromSeconds(10));
            connected = false;
            client = new TcpClient();
        }
        
        public void ConnectAndStart(string address, string accessKey)
        {
            if (string.IsNullOrEmpty(accessKey) || connected)
            {
                return;
            }
            this.Address = address;
            this.AccessKey = accessKey;

            try
            {
                client.Connect(Address, PORT);
                connected = true;
            } catch (SocketException)
            {
                connected = false;    
            }
        }

        public void Disconnect()
        {
            Address = null;
            AccessKey = null;
            client.Close();
        }
    }
}
