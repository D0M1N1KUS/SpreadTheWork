using System;

namespace Core.DataClasses
{
    [SerializableAttribute]
    public struct RunMethod
    {
        public Type type;
        public string memberName;
        public object[] args;
    }
}