using System;
using System.Collections.Concurrent;
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
            return _scheduler.QueueIsEmpty() && _scheduler.ClientsIdle();
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
            _scheduler.Quit();
        }

        public void LoadAssembly(string path)
        {
            Logger.Info("Initializing assembies on clients...");
            if (!_args.InitializationSucceeded)
                throw new Exception("Args not initialized properly!");
            
            var responses = SendInstantiationRequests(new LoadAssembly() { Path = path });
            CheckIfInstantiationsSucceeded(responses);
            Logger.Info("Instantiation of assembies succeeded!");
        }

        private static void CheckIfInstantiationsSucceeded(List<ISerializedData> receivedResponses)
        {
            var failedClients = 0;
            foreach (var response in receivedResponses)
            {
                if (!InstatntiationResponse.InstantiationSucceeded(ObjectSerializer.Deserialize(response)))
                    failedClients++;
            }

            if (failedClients > 0)
                throw new Exception("At least one client failed to initialize.");
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
    }
        
}