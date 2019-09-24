using System;
using NLog;

namespace Server
{
    internal class Server
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Server.log"};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;
            
            Logger.Info("Multi-Threaded TCP Server Demo");
            var server = new ServerBase.ServerTest1(5555);
            server.WaitForClient();
        }
    }
}