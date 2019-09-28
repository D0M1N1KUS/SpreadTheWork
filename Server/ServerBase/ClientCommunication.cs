using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

        public ConcurrentDictionary<int, NetworkStream> ConnectedClients { get; private set; }
        private ConcurrentDictionary<int, Thread> clientThreads;

        private ConcurrentQueue<IMessage> outgoingMessagesQueue;
        private ConcurrentQueue<IMessage> incommingMessagesQueue;

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
            ConnectedClients = new ConcurrentDictionary<int, NetworkStream>();
        }

        private void addClientCallback(int clientId, NetworkStream networkStream)
        {
            do
            {
                Logger.Debug($"Trying to add client with id {clientId}.");
            } while (!ConnectedClients.TryAdd(clientId, networkStream));
            Logger.Info($"Client with id {clientId} added.");
        }

        public void Send(IMessage message)
        {
            if (!ConnectedClients.ContainsKey(message.DestinationClient))
            {
                Logger.Error($"Tried to send message to unreachble client {message.DestinationClient}");
                return;
            }
            
            ConnectedClients[message.DestinationClient]
                .Write(message.SerializedData.data,0,message.SerializedData.data.Length);
        }

        public Task<ISerializedData> Receive(int clientId)
        {
            var task = new Task<ISerializedData>(() => receive(clientId));
            task.Start();

            return task;
        }

        private ISerializedData receive(int clientId)
        {
//            using (var streamReader = new StreamReader(connectedClients[clientId]))
//            {
//                streamReader.Read(,)
//            }
            var bytesList = new List<byte>();
            var buffer = new byte[1024];
            
            using (var binaryReader = new BinaryReader(ConnectedClients[clientId]))
            {
                var bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
                for (var i = 0; i < bytesRead; i++)
                    bytesList.Add(buffer[i]);
            }

            return new SerializedData() {data = bytesList.ToArray()};
        }
    }
}