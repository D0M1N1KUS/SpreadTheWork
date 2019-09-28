using System;
using System.Linq;
using System.Threading.Tasks;
using Client.ClientBase;
using Core.DataClasses;

namespace Client.ObjectSendingTest
{
    public class ObjectSendingTest
    {
        private static NLog.Logger logger;
        public static void TryToReceiveObjects(ServerCommunication c)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Info("Waiting for list...");
            var receivedObject = checkObject(c.Receive(), 10);
            logger.Info("Sending list back...");
            c.Send(receivedObject).Wait(10000);
            logger.Info("List sent! Wating for next list...");
            
            receivedObject = checkObject(c.Receive(), 1010);
            logger.Info("Sending list back...");
            c.Send(receivedObject).Wait(10000);
            logger.Info("Done! Test succeeded!");
        }

        private static object checkObject(object receivedObject, int itemCount)
        {
            logger.Info("Received list!");
            if(!(receivedObject is EncapsulatedList))
                throw new Exception($"Type missmatch: Expected {typeof(EncapsulatedList).Name}, but received {receivedObject.GetType().Name}.");

            var receivedList = (EncapsulatedList) receivedObject;
            if(receivedList.list.Count != itemCount)
                throw new Exception($"List cont missmatch: Expected 10, bur got {receivedList.list.Count}");
            
            var differences = receivedList.list.Where((t, i) => t != receivedList.list[i]).Count();
            if(differences > 0)
                throw new Exception($"Found {differences} differences in list.");
            
            NLog.LogManager.GetCurrentClassLogger().Info("Lists match!!!");
            return receivedObject;
        }
    }
}