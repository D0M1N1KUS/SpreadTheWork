using System;
using Core.DataClasses;
using Server.ObjectSender;

namespace Server.ResponseInterpreters
{
    public class RunMethodResponse
    {
        public object ResponseData { get; private set; } = null;
        public Exception Exception { get; private set; } = null;

        public bool CheckResponse(object response)
        {
            switch (response)
            {
                case MethodResponse methodResponse:
                    ResponseData = methodResponse.Object;
                    return true;
                case ErrorResponse errorResponse:
                    Exception = errorResponse.Exception;
                    return false;
                default:
                    throw new Exception($"Unexpected response type \"{response.GetType().Name}\"");
            }
        }
    }
}