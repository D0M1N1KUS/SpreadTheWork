using Server.Interfaces;

namespace Core.DataClasses
{
    public class Message : IMessage
    {
        public ISerializedData SerializedData { get; set; }
        public int DestinationClient { get; set; }
    }
}