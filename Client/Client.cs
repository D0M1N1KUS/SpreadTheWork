using System;
using System.Net;
using NLog;
using System.Runtime.Remoting;
using Client.ClientBase;

namespace Client
{
    internal class Client
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Client.log", DeleteOldFileOnStartup = true};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            try
            {
                runClient();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
            
            Logger.Info("Exitting...");
            Console.ReadLine();
        }

        private static void runClient()
        {
            Logger.Info("Client");
            Console.WriteLine("Server IP:");
            var ip = Console.ReadLine();
            if (!IPAddress.TryParse(ip, out _))
                throw new Exception("Unable to parse IP!");

            Console.WriteLine("Server Port:");
            var port = int.Parse(Console.ReadLine() ?? throw new Exception("Unable to parse server port!"));

//            var connector = new Connector();
//            var networkStream = connector.ConnectToServer(ip, port);
            var serverCommunication = new ServerCommunication(ip, port);

            Logger.Info("Starting test.");
            try
            {
                ObjectSendingTest.ObjectSendingTest.TryToReceiveObjects(serverCommunication);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}