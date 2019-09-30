using System;
using System.Net;
using System.Reflection;
using NLog;
using System.Runtime.Remoting;
using Client.ClientBase;

namespace Client
{
    internal class Client
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static string _assemblyLocation;
        
        public static void Main(string[] args)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile"){FileName = "Client.log", DeleteOldFileOnStartup = true};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            _assemblyLocation = $"{Assembly.GetExecutingAssembly().Location.Replace("Client.exe", String.Empty)}{args[0]}";
            
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

            var serverCommunication = new ServerCommunication(ip, port);
            var taskRunner = new TaskRunner.TaskRunner(serverCommunication, _assemblyLocation);
            taskRunner.BeginReceivingTasks();


            Logger.Info("Exitting...");
            Console.ReadLine();
        }
    }
}