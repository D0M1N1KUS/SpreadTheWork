using System;
using System.Threading;
using NLog;
using Server.Helpers;
using Server.ServerBase;
using Server.ObjectSendingTest;
using Server.TaskScheduling;
using ServerAssemblyInterface.Interfaces;

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
            try
            {
                if (!readArgs(args))
                    throw new Exception("Error in args!");

                var clientCommunication = WaitForClients();

                var taskRunner = new TaskRunner(clientCommunication, _args);

                var type = _args.LoadedAssembly.GetType($"{_args.LoadedAssembly.GetName().Name}.Algorithm");
                var obj = Activator.CreateInstance(type);
                var algorithm = (IStartAlgorithm) obj;

                algorithm.Run(taskRunner);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
//            Logger.Info("Starting test.");
//            try
//            {
//                ObjectSendingTest.ObjectSendingTest.TryToSendObjects(clientCommunication);
//            }
//            catch (Exception e)
//            {
//                Logger.Error(e);
//            }
            
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

        private static ClientCommunication WaitForClients()
        {
            var clientCommunictaion = new ClientCommunication();
            Logger.Info($"Waiting for {_args.ExpectedNumberOfClients} clients...");
            while(clientCommunictaion.ConnectedClients.Count < _args.ExpectedNumberOfClients)
                Thread.Sleep(100);
            Logger.Info("All clients connected!");
            return clientCommunictaion;
        }
    }
}