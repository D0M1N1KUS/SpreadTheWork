using System;
using System.Threading;
using NLog;
using Server.ServerBase;
using Server.ObjectSendingTest;

namespace Server
{
    internal class Server
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Server.log", DeleteOldFileOnStartup = true};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
            
            Logger.Info("Server");
//            var clientListeners = new ClientListener(1234);
//            var clientConnector = new ClientConnector();
//            clientListeners.AddConnectionCallback(clientConnector.ConnectClient);
            var clientCommunication = new ClientCommunication();
            while(clientCommunication.ConnectedClients.Count == 0)
                Thread.Sleep(100);
            
            Logger.Info("Starting test.");
            try
            {
                ObjectSendingTest.ObjectSendingTest.TryToSendObjects(clientCommunication);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            
            Logger.Info("Exitting...");
            Console.ReadLine();
        }
        
       
    }
}