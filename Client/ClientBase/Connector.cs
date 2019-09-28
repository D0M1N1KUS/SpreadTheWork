using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Core.DataClasses;
using NLog;
using Server.ObjectSender;

namespace Client.ClientBase
{
    public class Connector
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private TcpClient _client;
        
        private int clientId;
        
        public bool connectionSucceeded { get; private set; } = false;
        public NetworkStream serverNetworkStream { get; private set; }
        
        public Connector()
        {
            _client = new TcpClient();
        }
        
        public NetworkStream ConnectToServer(string ipAddress, int portNum)
        {
            _client.Connect(ipAddress, portNum);

            var clientListenerStream = _client.GetStream();
            
            clientListenerStream.ReadTimeout = 10000;
            var buffer = new byte[1024];
            var bytesRead = clientListenerStream.Read(buffer, 0, buffer.Length);
            Logger.Debug("Server responded.");

            var response = ObjectSerializer.Deserialize(buffer);
            
            if(!(response is ConnectionResponse))
                throw new Exception($"Unexpected response type from server. Expected {typeof(ConnectionResponse).Name}, but got {response.GetType().Name}.");

            var connectionResponse = (ConnectionResponse) response;
            
            if(!connectionResponse.ConnectionAccepted)
                throw new Exception($"Server refused connection!");

            Logger.Debug($"Server accepted connection on port {connectionResponse.port}. Id of client is {connectionResponse.clientID}.");
            clientId = connectionResponse.clientID;

            Logger.Debug($"Connecting to server on port {connectionResponse.port}...");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Parse(ipAddress), connectionResponse.port);
            
            serverNetworkStream = new NetworkStream(socket, true);

            var responseForServer = ObjectSerializer.SerializeToBytes(connectionResponse);            
            Logger.Debug("Sending connection response to server.");
            serverNetworkStream.Write(responseForServer,0, responseForServer.Length);
            
            Logger.Info("Connection established!");
            connectionSucceeded = true;
            return serverNetworkStream;
        }
    }
}