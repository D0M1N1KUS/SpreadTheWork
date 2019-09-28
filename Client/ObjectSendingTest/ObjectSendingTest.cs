using System;
using System.Linq;
using System.Threading.Tasks;
using Client.ClientBase;
using Core.DataClasses;

namespace Client.ObjectSendingTest
{
    public class ObjectSendingTest
    {
        public static void TryToReceiveObjects(ServerCommunication c)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            var receivedObject = checkObject(c.Receive(), 10);
            c.Send(receivedObject);
            
            receivedObject = checkObject(c.Receive(), 1010);
            c.Send(receivedObject);
            
        }

        private static object checkObject(object receivedObject, int itemCount)
        {
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