using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Core.DataClasses;
using NLog;
using Server.Helpers;
using Server.ObjectSender;

namespace Server.ServerBase
{
    public class ClientConnector
    {
        private const int DEFAULT_CLIENT_START_PORT = 3022;
        private const int MAX_PORT = 4000;
        
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private int currentPort = DEFAULT_CLIENT_START_PORT;
        private int currentClientId = 1;

        private ObjectSerializer _objectSerializer;
        
        private ConcurrentDictionary<int, NetworkStream> connectedClients;

        private object clientConnectorLock = new object();

        public ClientConnector()
        {
            _objectSerializer = new ObjectSerializer();
            connectedClients = new ConcurrentDictionary<int, NetworkStream>();
        }
        
        public void ConnectClient(TcpClient client)
        {
            try
            {
                lock (clientConnectorLock)
                {
                    connect(client);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to connect to client with ip {((IPEndPoint) client.Client.RemoteEndPoint).Address}. Reason\n{e.Message}");
            }
        }

        private void connect(TcpClient client)
        {
            var clientId = sendConnectionResponseToClient(client, out var clientStream);
            waitForClientResponse(clientStream, clientId, out var buffer);

            var response = ObjectSerializer.Deserialize(buffer);
            if (!(response is ConnectionResponse))
            {
                Logger.Error($"Inconsistent response from client {clientId}. Expected \"{typeof(ConnectionResponse).Name}\" but got \"{response.GetType().Name}\".");
                return;
            }

            var connectionResponse = (ConnectionResponse) response;
            if (connectionResponse.clientID != clientId)
            {
                Logger.Error($"Unexpected client id. Expected {clientId}, but got {connectionResponse.clientID}.");
                return;
            }

            if (!connectionResponse.ConnectionAccepted)
            {
                Logger.Info($"Client with id {clientId} refused to connect!");
                return;
            }

            do
            {
                Logger.Debug($"Trying to add client with id {clientId}.");
            } while (!connectedClients.TryAdd(clientId, clientStream));
            Logger.Info($"Client with id {clientId} added.");
        }

        private static void waitForClientResponse(NetworkStream clientStream, int clientId, out byte[] buffer)
        {
            //var currentClient = new NetworkStream(clientStream, true) {ReadTimeout = 10000};

            Logger.Debug($"Accept message sent to client {clientId}. Waiting for response...");
            buffer = new byte[1024];
            var bytesRead = clientStream.Read(buffer, 0, buffer.Length);
        }

        private int sendConnectionResponseToClient(TcpClient client, out NetworkStream clientStream)
        {
            var port = currentPort++;//getNextFreePort();

            var currentNetworkStream = client.GetStream();
            var clientId = currentClientId++;
            var connectionMessage = ObjectSerializer.SerializeToBytes(
                new ConnectionResponse() {clientID = clientId, port = port, ConnectionAccepted = true});
            Logger.Debug($"Sending accept response to client {clientId}...");
            currentNetworkStream.Write(connectionMessage, 0, connectionMessage.Length);

            Logger.Debug("Creating client listener...");
            var clientIp = ((IPEndPoint) client.Client.RemoteEndPoint).Address;
            var clientListener = new TcpListener(clientIp, port);
            clientListener.Start();
            Logger.Debug($"Wating for client on port {port}...");
            clientStream = clientListener.AcceptTcpClient().GetStream();//clientSocket.Connect(clientIp, port);
            
            return clientId;
        }

        private int getNextFreePort()
        {
            while (currentPort <= MAX_PORT && !PortChecker.CheckIfPortIsOpen(currentPort))
                currentPort++;
            
            if(currentPort > MAX_PORT)
                throw new Exception("Exceeded range of ports!");

            return currentPort;
        }
    }
}