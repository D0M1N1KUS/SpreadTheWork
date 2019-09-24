using System;

namespace Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Multi-Threaded TCP Server Demo");
            var server = new ServerBase.ServerTest1(5555);
            server.WaitForClient();
        }
    }
}