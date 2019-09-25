using System;
using System.Threading;
using NLog;
using Server.ServerBase;

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
            var clientListeners = new ClientListener(1234);
            var clientConnector = new ClientConnector();
            clientListeners.AddConnectionCallback(clientConnector.ConnectClient);
            
            while(true)
                Thread.Sleep(100);

            Console.ReadLine();
        }
    }
}