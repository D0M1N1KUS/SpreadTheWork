using System;
using System.Collections.Generic;
using System.Linq;
using Core.DataClasses;
using Server.Interfaces;
using Server.ObjectSender;
using Server.ServerBase;

namespace Server.ObjectSendingTest
{
    public static class ObjectSendingTest
    {
        private static int number = 0;
        private static EncapsulatedList listToSend = new EncapsulatedList(){list = new List<int>()};
        
        public static void TryToSendObjects(ClientCommunication c)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            
            for (var i = 1; i <= 10; i++)
            {
                listToSend.list.Add(number++);
            }

            var destinationClient = c.ConnectedClients.First().Key;
            var message = new Message()
            {
                DestinationClient = destinationClient,
                SerializedData = ObjectSerializer.Serialize(listToSend)
            };
            
            c.Send(message);
            var receiveTask = c.Receive(destinationClient);
            receiveTask.Wait(10000);
            checkResponse(receiveTask.Result, logger);
            
            for (var i = 1; i <= 1000; i++)
            {
                listToSend.list.Add(number++);
            }

            message = new Message()
            {
                DestinationClient = destinationClient,
                SerializedData = ObjectSerializer.Serialize(listToSend)
            };
            
            c.Send(message);
            receiveTask = c.Receive(destinationClient);
            receiveTask.Wait(10000);
            checkResponse(receiveTask.Result, logger);
        }

        private static void checkResponse(ISerializedData d, NLog.Logger logger)
        {
            var deserializedData = ObjectSerializer.Deserialize(d.data);
            if(!(deserializedData is EncapsulatedList))
                throw new Exception($"Type missmatch: Expected {typeof(EncapsulatedList).Name}, but received {deserializedData.GetType().Name}.");

            var receivedList = deserializedData as EncapsulatedList;
            
            if (receivedList.list?.Count != listToSend.list.Count)
                throw new Exception($"List sized differ!");

            var differences = listToSend.list.Where((t, i) => t != receivedList.list[i]).Count();
            if(differences > 0)
                throw new Exception($"Found {differences} differences in list.");
            
            logger.Info("Lists match!!!");
        }

    }
}