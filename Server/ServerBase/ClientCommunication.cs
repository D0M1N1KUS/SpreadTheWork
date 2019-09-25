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

        private ConcurrentDictionary<int, NetworkStream> connectedClients;
        private ConcurrentDictionary<int, Thread> clientThreads;

        private ConcurrentQueue<IMessage> outgoingMessagesQueue;
        private ConcurrentQueue<IMessage> incommingMessagesQueue;

        public ClientCommunication(ClientListener listener = null, ClientConnector connector = null)
        {
            _listener = listener ?? new ClientListener(DEFAULT_LISTENING_PORT);
            _connector = connector ?? new ClientConnector(addClientCallback);
            connectedClients = new ConcurrentDictionary<int, NetworkStream>();
        }

        private void addClientCallback(int clientId, NetworkStream networkStream)
        {
            do
            {
                Logger.Debug($"Trying to add client with id {clientId}.");
            } while (!connectedClients.TryAdd(clientId, networkStream));
            Logger.Info($"Client with id {clientId} added.");

//            do
//            {
//                
//            } while (!clientThreads.TryAdd(clientId)});
        }

        public void Send(IMessage message)
        {
            if (!connectedClients.ContainsKey(message.DestinationClient))
            {
                Logger.Error($"Tried to send message to unreachble client {message.DestinationClient}");
                return;
            }
            
            connectedClients[message.DestinationClient]
                .Write(message.SerializedData.data,0,message.SerializedData.data.Length);
        }

        public Task<IMessage> Receive(int clientId)
        {
            var task = new Task<IMessage>(() => receive(clientId));
            
        }

        private ISerializedData receive(int clientId)
        {
//            using (var streamReader = new StreamReader(connectedClients[clientId]))
//            {
//                streamReader.Read(,)
//            }
            var bytesList = new List<byte>();
            var buffer = new byte[1024];
            
            using (var binaryReader = new BinaryReader(connectedClients[clientId]))
            {
                var bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
                for (var i = 0; i < bytesRead; i++)
                    bytesList.Add(buffer[i]);
            }

            return new SerializedData() {data = bytesList.ToArray()};
        }
    }
}