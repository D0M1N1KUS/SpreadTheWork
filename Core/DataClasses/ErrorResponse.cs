using System;

namespace Core.DataClasses
{
    [SerializableAttribute]
    public struct ErrorResponse
    {
        public Exception Exception;
    }
}