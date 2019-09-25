using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.ServerBase
{
    public class ClientListener
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private TcpListener _listener;
        private bool _listen;

        private Thread _listenerThread;
        private event ClientConnectedCallbackEvent _clientConnectedEvent;

        public ClientListener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listen = true;
            _listenerThread = new Thread(listenForClients);
            _listenerThread.Start();
            Logger.Info($"Listening for clients on port {port}.");
        }

        ~ClientListener()
        {
            if(_listenerThread.IsAlive)
                _listenerThread.Abort();
        }

        public void AddConnectionCallback(ClientConnectedCallbackEvent callbackEvent)
        {
            Logger.Debug($"{callbackEvent.Method.Name} subscribed to client connection event");
            _clientConnectedEvent += callbackEvent;
        }

        public void RemoveConnectionCallback(ClientConnectedCallbackEvent callbackEvent)
        {
            Logger.Debug($"{callbackEvent.Method.Name} unsubscribed to client connection event");
            _clientConnectedEvent -= callbackEvent;
        }

        public void StopListening()
        {
            Logger.Debug("Stopping Client listener...");
            _listenerThread.Abort("Stopped by user.");
            _listen = false;
        }

        private void listenForClients()
        {
            try
            {
                listen();
            }
            catch (ThreadAbortException e)
            {
                Logger.Debug($"Client listener thread aborted - {(string)e.ExceptionState}");
                return;
            }
            catch (Exception e)
            {
                Logger.Debug($"Client listener exception\n{e.Message}");
            }
            Logger.Debug("Client listener stopped.");
        }

        private void listen()
        {
            while (_listen)
            {
                _listener.Start();
                var tcpClient = _listener.AcceptTcpClient();
                var ipAddress = ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address;
                Logger.Debug($"Connection attempt from [{ipAddress}]");
                _clientConnectedEvent?.Invoke(tcpClient);
            }
        }

        public delegate void ClientConnectedCallbackEvent(TcpClient client);
        
    }
}