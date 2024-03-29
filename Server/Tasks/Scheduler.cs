using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Core.DataClasses;
using Server.Interfaces;
using Server.ObjectSender;
using Server.ResponseInterpreters;
using Server.ServerBase;

namespace Server.TaskScheduling
{
    public class Scheduler
    {
        private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private ClientCommunication _clientCommunication;
        
        public ConcurrentDictionary<long, object> FinishedTasks { get; private set; }

        private ConcurrentQueue<Tuple<long, ISerializedData>> _requestsQueue;
        private ConcurrentDictionary<int, bool> _clientIsBusyDictionary;

        private long taskId = 0;

        private readonly List<Thread> _serverThreads;

        public Scheduler(ClientCommunication clientCommunication)
        {
            _clientCommunication = clientCommunication;
            FinishedTasks = new ConcurrentDictionary<long, object>();
            _requestsQueue = new ConcurrentQueue<Tuple<long, ISerializedData>>();
            _serverThreads = new List<Thread>();
            var clientIsBusyDictionary = new Dictionary<int, bool>();
            foreach (var client in _clientCommunication.ConnectedClients)
            {
                var thread = new Thread(new ParameterizedThreadStart(serverThreadMethod));
                thread.Start(client.Value);
                _serverThreads.Add(thread);
                clientIsBusyDictionary.Add(client.Key, false);
            }
            
            _clientIsBusyDictionary = new ConcurrentDictionary<int, bool>(clientIsBusyDictionary);
        }

        ~Scheduler()
        {
            Quit();
        }

        public long QueueTask(ISerializedData message)
        {
            var currentTaskId = taskId++;
            _requestsQueue.Enqueue(new Tuple<long, ISerializedData>(currentTaskId, message));
            return currentTaskId;
        }

        public bool QueueIsEmpty()
        {
            return _requestsQueue.IsEmpty;
        }

        public void Quit()
        {
            foreach (var thread in _serverThreads)
            {
                if(thread.IsAlive)
                    thread.Abort();
            }
        }

        public bool ClientsIdle()
        {
            foreach (var client in _clientIsBusyDictionary)
            {
                if (client.Value)
                    return false;
            }

            return true;
        }
        
        private void serverThreadMethod(object obj)
        {
            var client = (Client)obj;
            while (true)
            {
                try
                {
                    while (_requestsQueue.IsEmpty)
                        Thread.Sleep(10);

                    Logger.Debug("Getting next task...");
                    if (_requestsQueue.TryDequeue(out var data))
                    {
                        _clientIsBusyDictionary[client.Id] = true;
                        _clientCommunication
                            .Send(new Message() {DestinationClient = client.Id, SerializedData = data.Item2});

                        var serializedData = _clientCommunication.Receive(client.Id);
                        var response = new RunMethodResponse();
                        if (response.CheckResponse(ObjectSerializer.Deserialize(serializedData)))
                            FinishedTasks.TryAdd(data.Item1, response.ResponseData);
                        else
                            Logger.Error(response.Exception);
                    }
                    _clientIsBusyDictionary[client.Id] = false;
                }
                catch (ThreadAbortException ta)
                {
                    Logger.Debug(ta);
                    Logger.Info($"Exiting server thread for client {client.Id}...");
                    return;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
        
    }
}