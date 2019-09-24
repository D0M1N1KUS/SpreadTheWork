using System;
using System.Net;
using System.Net.Sockets;

namespace Server.Helpers
{
    public static class PortChecker
    {
        public static bool CheckIfPortIsOpen(int port)
        {
            using(var tcpClient = new TcpClient())
            try
            {
                tcpClient.Connect(IPAddress.Any, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}