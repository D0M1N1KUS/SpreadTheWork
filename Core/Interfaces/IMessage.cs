using System.Net;

namespace Server.Interfaces
{
    public interface IMessage
    {
        ISerializedData SerializedData { get; set; }
        int DestinationClient { get; set; }
    }
}