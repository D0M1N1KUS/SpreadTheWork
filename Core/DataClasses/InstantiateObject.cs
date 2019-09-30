using System;

namespace Core.DataClasses
{
    [SerializableAttribute]
    public struct InstantiateObject
    {
        public Type type;
        public object[] args;
    }
}