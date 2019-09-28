using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Core.DataClasses;
using Server.Interfaces;
using Server.ObjectSender;

namespace Client.ClientBase
{
    public class ServerCommunication
    {
        private Connector _serverConnector;

        private BinaryReader _binaryReader;

        private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        public Task Send(object objectToSend)
        {
            var task = new Task(() => send(objectToSend));
            task.Start();
            return task;
        }
        
        private void send(object objectToSend)
        {
            var serializedData = ObjectSerializer.Serialize(objectToSend);
            if(!_serverConnector.connectionSucceeded)
                throw new Exception("Can't send data, connection has not been established");
            
            var messageInfoSerialized = ObjectSerializer
                .Serialize(new MessageInfo(){LengthInBytes = serializedData.data.Length});
            _serverConnector.serverNetworkStream
                .Write(messageInfoSerialized.data, 0, messageInfoSerialized.data.Length);
            
            _serverConnector.serverNetworkStream
                .Write(serializedData.data,0,serializedData.data.Length);
            Logger.Info($"Sent {serializedData.data.Length} bytes.");
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
            var buffer = new byte[Core.CommonData.NETWORK_BUFFER_SIZE];
            var bytesRead = 0;

//            while (_binaryReader.)
//            {
//                bytesRead = _binaryReader.Read(buffer, 0, buffer.Length);
//                for (var i = 0; i < bytesRead; i++)
//                    bytesList.Add(buffer[i]);
//            } 

            bytesRead = _serverConnector.serverNetworkStream.Read(buffer, 0, buffer.Length);
            var infoMessage = (MessageInfo)ObjectSerializer.Deserialize(buffer);

            for(var i = 0; i <= infoMessage.LengthInBytes / Core.CommonData.NETWORK_BUFFER_SIZE; i++)
            {
                bytesRead = _binaryReader.Read(buffer, 0, buffer.Length);
                for (var j = 0; j < bytesRead; j++)
                    bytesList.Add(buffer[j]);
            }

            Logger.Info($"Received {bytesList.Count} bytes.");
            return ObjectSerializer.Deserialize(bytesList.ToArray());
        }
    }
}