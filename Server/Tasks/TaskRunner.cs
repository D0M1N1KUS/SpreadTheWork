using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Core.DataClasses;
using Server.Interfaces;
using Server.ObjectSender;
using Server.ServerBase;
using ServerAssemblyInterface.Interfaces;
using Server.ResponseInterpreters;
using Server.Helpers;

namespace Server.TaskScheduling
{
    public class TaskRunner : IRunner
    {
        private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private ClientCommunication _clientCommunication;
        private Scheduler _scheduler;
        private Args _args;

        public TaskRunner(ClientCommunication clientCommunication, Args args)
        {
            _clientCommunication = clientCommunication;
            _scheduler = new Scheduler(clientCommunication);
            _args = args;
        }


        public void InstantiateObject(Type type, object[] args)
        {
            Logger.Info($"Instantiating object of type {type.Name} in clients...");
            var instantiateObject = new InstantiateObject() { type = type, args = args };
            var instantiationResponses = SendInstantiationRequests(instantiateObject);
//            WaitForSendTasksToFinish(sentInstantiationRequests);
//
//            var receivedResponses = ReceiveResponses();
//            WaitForClientsToRespond(receivedResponses);
            CheckIfInstantiationsSucceeded(instantiationResponses);
            Logger.Info("Instantiation of objects succeeded!");
        }
        
        public long RunFunction(Type objectType, string memberName, object[] args)
        {
            return _scheduler.QueueTask(ObjectSerializer.Serialize(new RunMethod()
            {
                type = objectType,
                memberName = memberName,
                args = args
            }));
        }
        
        public object GetResult(long taskId)
        {
            return _scheduler.FinishedTasks.ContainsKey(taskId)
                ? _scheduler.FinishedTasks[taskId]
                : null;
        }

        public bool TasksCompleted()
        {
            return _scheduler.QueueIsEmpty();
        }

        public void Finish()
        {
            foreach (var client in _clientCommunication.ConnectedClients)
            {
                _clientCommunication.Send(new Message()
                {
                    DestinationClient = client.Value.Id,
                    SerializedData = ObjectSerializer.Serialize(new Shutdown())
                });
            }
        }

        public void LoadAssembly(string path)
        {
            Logger.Info("Initializing assembies on clients...");
            if (!_args.InitializationSucceeded)
                throw new Exception("Args not initialized properly!");
            //Assembly needs to be in the same directory as executable
            //var assemblyPath = args.LoadedAssembly.GetName().Name;
//            Logger.Debug("Getting responses from clients...");
//            var responseTasks = ReceiveResponses();
//            Logger.Debug("Sending instantiation requests...");
            var responses = SendInstantiationRequests(new LoadAssembly() { Path = path });
//            Logger.Debug("Waiting for send tasks to finish...");
//            WaitForSendTasksToFinish(sendTasks);
//
//            Logger.Debug("Waiting for clients to respond...");
//            WaitForClientsToRespond(responseTasks);
//            Logger.Debug("Checking respones from clients...");
            CheckIfInstantiationsSucceeded(responses);
            Logger.Info("Instantiation of assembies succeeded!");
        }

        private static void CheckIfInstantiationsSucceeded(List<ISerializedData> receivedResponses)
        {
            var failedClients = 0;
            foreach (var response in receivedResponses)
            {
                if (!InstatntiationResponse.InstantiationSucceeded(response))
                    failedClients++;
            }

            if (failedClients > 0)
                throw new Exception("At least one client failed to initialize.");
        }

        private static void WaitForSendTasksToFinish(List<Task> sendTasks)
        {
            foreach (var task in sendTasks)
            {
                task.Wait(10000);
            }
        }

        private static void WaitForClientsToRespond(List<Task<ISerializedData>> receivedResponses)
        {
            foreach (var task in receivedResponses)
                task.Wait(10000);
        }

        private List<ISerializedData> SendInstantiationRequests(object instantiateObject)
        {
            var responses = new List<ISerializedData>();
            foreach (var client in _clientCommunication.ConnectedClients)
            {
                Logger.Debug("Sending instantiation request...");
                _clientCommunication.Send(new Message()
                {
                    DestinationClient = client.Value.Id,
                    SerializedData = ObjectSerializer.Serialize(instantiateObject)
                });
                Logger.Debug("Waiting for response...");
                var response = _clientCommunication.Receive(client.Value.Id);
                Logger.Debug("Received response!");
                responses.Add(response);
            }

            return responses;
        }

        private List<Task<ISerializedData>> ReceiveResponses()
        {
            var receiveTasks = new List<Task<ISerializedData>>();
            foreach (var client in _clientCommunication.ConnectedClients)
            {
                var task = _clientCommunication.ReceiveTask(client.Key);
                task.Start();
                receiveTasks.Add(task);
            }
            return receiveTasks;
        }

        private bool instantiationSucceeded(int clientId)
        {
            var task = _clientCommunication.ReceiveTask(clientId);
            task.Wait();

            var succeeded = InstatntiationResponse.InstantiationSucceeded(ObjectSerializer.Deserialize(task.Result));
            if (!succeeded)
                Logger.Error(InstatntiationResponse.Exception);
            return succeeded;
        }
    }
        
}