using System;
using Core.DataClasses;
using Server.ObjectSender;

namespace Server.ResponseInterpreters
{
    public static class InstatntiationResponse
    {
        public static string SuccessMessage { get; private set; }
        public static Exception Exception { get; private set; }
        
        public static bool InstantiationSucceeded(object response)
        {
            switch (response)
            {
                case SuccessResponse successResponse:
                    SuccessMessage = successResponse.message;
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