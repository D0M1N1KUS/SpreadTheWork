using System;
using System.Runtime.Serialization;
using Server.Interfaces;

namespace Server.ObjectSender
{
    [SerializableAttribute]
    public class SerializedObject : ISerializableData
    {
        public byte[] data { get; set; }
    }
}