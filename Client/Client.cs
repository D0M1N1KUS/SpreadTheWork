using System;
using NLog;
using System.Runtime.Remoting;

namespace Client
{
    internal class Client
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Server.log"};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
            
            Console.WriteLine("Multi-Threaded TCP Server Demo");
            Console.WriteLine("Provide IP:");
            var ip = Console.ReadLine();
 
            Console.WriteLine("Provide Port:");
            var port = int.Parse(Console.ReadLine() ?? throw new Exception("null"));
 
            var client = new ClientBase.ClientTest1(ip, port);
            client.HandleCommunication();
        }
    }
}