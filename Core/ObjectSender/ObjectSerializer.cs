using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Server.Interfaces;

namespace Server.ObjectSender
{
    public class ObjectSerializer
    {
        public static ISerializableData Serialize(object serializableObject)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, serializableObject);
                return new SerializedObject {data = memoryStream.ToArray()};
            }
        }

        public static object Deserialize(ISerializableData data)
        {
            using (var memorySteam = new MemoryStream(data.data))
                return (new BinaryFormatter()).Deserialize(memorySteam);
        }
    }
}