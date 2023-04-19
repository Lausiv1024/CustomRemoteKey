using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CustomRemoteKey.Networking
{
    internal class ServerSync
    {
        Socket server;
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 60001);
        public ServerSync() 
        {
            server = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        }

        public async Task Run()
        {
            try
            {

            } catch
            {

            }
        }
    }
}
