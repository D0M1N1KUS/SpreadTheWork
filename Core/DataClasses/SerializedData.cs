using Server.Interfaces;

namespace Core.DataClasses
{
    public class SerializedData : ISerializedData
    {
        public byte[] data { get; set; }
    }
}