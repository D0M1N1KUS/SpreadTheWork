using System;
using System.Threading;
using NLog;
using Server.Helpers;
using Server.ServerBase;
using Server.ObjectSendingTest;

namespace Server
{
    internal class Server
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static Args _args;
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Server.log", DeleteOldFileOnStartup = true};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
            
            Logger.Info("Server");
            
            readArgs(args);
            
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

        private static bool readArgs(string[] args)
        {
            _args = new Args(args);
//            if(!_args.InitializetionSucceeded)
//                Logger.Error($"Error in provided arguments!");
            return _args.InitializationSucceeded;
        }
    }
}