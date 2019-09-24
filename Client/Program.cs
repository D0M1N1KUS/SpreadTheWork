using System;
using System.Runtime.Remoting;

namespace Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
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