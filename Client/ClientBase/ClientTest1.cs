using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Client.ClientBase
{
    public class ClientTest1
    {
        private TcpClient _client;
 
        private StreamReader _sReader;
        private StreamWriter _sWriter;
 
        private Boolean _isConnected;
 
        public ClientTest1(String ipAddress, int portNum)
        {
            _client = new TcpClient();
            _client.Connect(ipAddress, portNum);
        }
 
        public void HandleCommunication()
        {
            _sReader = new StreamReader(_client.GetStream(), Encoding.ASCII);
            _sWriter = new StreamWriter(_client.GetStream(), Encoding.ASCII);
 
            _isConnected = true;
            String sData = null;
            while (_isConnected)
            {
                Console.Write("&gt; ");
                sData = Console.ReadLine();
 
                _sWriter.WriteLine(sData);
                _sWriter.Flush();
            }
        }
    }
}