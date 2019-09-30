using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Core.DataClasses;
using Server.Interfaces;
using Server.ObjectSender;

namespace Server.ServerBase
{
    public class ClientCommunication
    {
        private const int DEFAULT_LISTENING_PORT = 1234;
        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private ClientListener _listener;
        private ClientConnector _connector;

        private ConcurrentDictionary<int, BinaryReader> BinaryReaders;

        public ConcurrentDictionary<int, Client> ConnectedClients { get; private set; }

        /// <summary>
        /// Initializes basic server-client communication and listener classes
        /// </summary>
        /// <param name="listener">Optional. Allows to add client listener with custom listening port.</param>
        /// <param name="connector">Optional. Allows to add client connector with custom AddConnectionCallback.</param>
        public ClientCommunication(ClientListener listener = null, ClientConnector connector = null)
        {
            _listener = listener ?? new ClientListener(DEFAULT_LISTENING_PORT);
            _connector = connector ?? new ClientConnector(addClientCallback);
            _listener.AddConnectionCallback(_connector.ConnectClient);
            ConnectedClients = new ConcurrentDictionary<int, Client>();
            BinaryReaders = new ConcurrentDictionary<int, BinaryReader>();
        }

        private void addClientCallback(int clientId, Client client)
        {
            do
            {
                Logger.Debug($"Trying to add client with id {clientId}.");
            } while (!ConnectedClients.TryAdd(clientId, client));
            Logger.Info($"Client with id {clientId} added.");

            do
            {
                Thread.Sleep(10);
            } while (!BinaryReaders.TryAdd(clientId, new BinaryReader(client.NetworkStream)));
        }

        public Task SendTask(IMessage message)
        {
            var task = new Task(() => Send(message));
            task.Start();
            return task;
        }
        
        public void Send(IMessage message)
        {
            if (!ConnectedClients.ContainsKey(message.DestinationClient))
            {
                Logger.Error($"Tried to send message to unreachble client {message.DestinationClient}");
                return;
            }
            
            var messageInfoSerialized = ObjectSerializer
                .Serialize(new MessageInfo(){LengthInBytes = message.SerializedData.data.Length});
            ConnectedClients[message.DestinationClient].NetworkStream
                .Write(messageInfoSerialized.data, 0, messageInfoSerialized.data.Length);
            
            ConnectedClients[message.DestinationClient].NetworkStream
                .Write(message.SerializedData.data,0,message.SerializedData.data.Length);
            Logger.Debug($"Sent {message.SerializedData.data.Length} bytes.");
        }

        public Task<ISerializedData> ReceiveTask(int clientId)
        {
            var task = new Task<ISerializedData>(() => Receive(clientId));
            task.Start();

            return task;
        }

        public ISerializedData Receive(int clientId)
        {
            var bytesList = new List<byte>();
            var buffer = new byte[Core.CommonData.NETWORK_BUFFER_SIZE];
            var bytesRead = 0;

            var clientNetworkStream = ConnectedClients[clientId].NetworkStream;
            
            bytesRead = clientNetworkStream.Read(buffer, 0, buffer.Length);
            var infoMessage = (MessageInfo)ObjectSerializer.Deserialize(buffer);
            

            for(var i = 0; i <= infoMessage.LengthInBytes / Core.CommonData.NETWORK_BUFFER_SIZE; i++)
            {
                bytesRead = ConnectedClients[clientId].NetworkStream.Read(buffer, 0, buffer.Length);
                for(var j = 0; j < bytesRead; j++)
                    bytesList.Add(buffer[j]);
            }

            Logger.Debug($"Received {bytesList.Count} bytes.");
            return new SerializedData() {data = bytesList.ToArray()};
        }
    }
}