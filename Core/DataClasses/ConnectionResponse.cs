using System;

namespace Core.DataClasses
{
    [SerializableAttribute]
    public struct ConnectionResponse
    {
        public int clientID;
        public int port;
        public bool ConnectionAccepted;
        public int Threads;
    }
}