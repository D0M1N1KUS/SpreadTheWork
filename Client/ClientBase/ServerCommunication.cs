using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Server.Interfaces;
using Server.ObjectSender;

namespace Client.ClientBase
{
    public class ServerCommunication
    {
        private Connector _serverConnector;

        private BinaryReader _binaryReader;

        public ServerCommunication(Connector serverConnector)
        {
            _serverConnector = serverConnector;
            if (!_serverConnector.connectionSucceeded)
                throw new Exception("Provided server connector did not connect of failed to connect");
            _binaryReader = new BinaryReader(_serverConnector.serverNetworkStream);
        }

        public ServerCommunication(string IPAddress, int port)
        {
            _serverConnector = new Connector();
            _serverConnector.ConnectToServer(IPAddress, port);
            _binaryReader = new BinaryReader(_serverConnector.serverNetworkStream);
        }

        public void Send(object objectToSend)
        {
            var serializedData = ObjectSerializer.Serialize(objectToSend);
            if(!_serverConnector.connectionSucceeded)
                throw new Exception("Can't send data, connection has not been established");
            _serverConnector.serverNetworkStream.Write(serializedData.data,0,serializedData.data.Length);
        }

        public Task<object> ReceiveTask()
        {
            var task = new Task<object>(Receive);
            task.Start();
            return task;
        }
        
        public object Receive()
        {
            var bytesList = new List<byte>();
            var buffer = new byte[1024];

            var bytesRead = _binaryReader.Read(buffer, 0, buffer.Length);
            for(var i = 0; i < bytesRead; i++)
                bytesList.Add(buffer[i]);

            return ObjectSerializer.Deserialize(bytesList.ToArray());
        }
    }
}