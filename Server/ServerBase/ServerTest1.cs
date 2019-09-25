using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;

namespace Server.ServerBase
{
    public class ServerTest1
    {
        private TcpListener _listener;
        private bool _isRunning;

        //private List<TcpClient> _clients;

        public ServerTest1(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;
        }

        public void WaitForClient()
        {
            while (_isRunning)
            {
                var tcpClient = _listener.AcceptTcpClient();
                Console.WriteLine("Found client!");
                var t = new Thread(new ParameterizedThreadStart(FoundClientCallback));
                t.Start(tcpClient);
            }
        }
 
        private void FoundClientCallback(Object obj)
        {
            if(!(obj is TcpClient))
                throw new Exception($"Unsupported client type: {obj.GetType()}.");
            var client = (TcpClient) obj;
            
            var sr = new StreamReader(client.GetStream(), Encoding.ASCII);
            var sw = new StreamWriter(client.GetStream(), Encoding.ASCII);

            var clientConnected = true;
            var data = string.Empty;

            while (clientConnected)
            {
                data = sr.ReadLine();
                Console.WriteLine(data);
            }
        }
    }
}