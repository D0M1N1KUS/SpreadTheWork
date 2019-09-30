using System.Net.Sockets;

namespace Server.ServerBase
{
    public struct Client
    {
        public int Id;
        public int Port;
        public NetworkStream NetworkStream;
        public int NumberOfThreads;
    }
}