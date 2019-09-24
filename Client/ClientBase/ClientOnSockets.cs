using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.ClientBase
{
    public class ClientOnSockets
    {
        private IPEndPoint serverIp;
        
        public bool Connected { get; private set; }

        public ClientOnSockets(string serverAddress, int serverPort = 1234)
        {
            serverIp = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
        }

        public void SendToServer(string input)
        {
            SendToServer(Encoding.ASCII.GetBytes(input));
        }

        public void SendToServer(byte[] data)
        {
            using (var senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp))
            {
                try
                {
                    senderSocket.Connect(serverIp);
                    senderSocket.Send(data);
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine($"Data is null!\n{ae.Message}");
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"Connection to server lost or not established!\n{se.Message}");
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine($"Connection to server lost!\n{e.Message}");
                }
            }
        }

        public byte[] WaitForResponse()
        {
            using (var receiverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                ProtocolType.Tcp))
            {
                receiverSocket.Connect(serverIp);
                var data = new byte[65536];
                var length = receiverSocket.Receive(data);
                var receivedData = new byte[length];
                Array.Copy(data, receivedData, length);
                return receivedData;
            }
        }
    }
}