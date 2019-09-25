using System;
using System.Runtime.Serialization;
using Server.Interfaces;

namespace Server.ObjectSender
{
    [SerializableAttribute]
    public class SerializedObject : ISerializedData
    {
        public byte[] data { get; set; }
    }
}