using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Client.ClientBase;
using Core.DataClasses;
using NLog;
using NLog.Fluent;
using Server.ObjectSender;

namespace Client.TaskRunner
{
    public class TaskRunner
    {
        private NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private ServerCommunication _serverCommunication;

        private Assembly _targetAssembly;

        private object calculationObject;

        public TaskRunner(ServerCommunication serverCommunication)
        {
            _serverCommunication = serverCommunication;
        }

        public void BeginReceivingTasks()
        {
            while (true)
            {
                var task = _serverCommunication.Receive();

                try
                {
                    if (runTask(task)) return;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    _serverCommunication.Send(new ErrorResponse(){Exception = e});
                }
            }
        }

        private bool runTask(object task)
        {
            switch (task)
            {
                case InstantiateObject instantiateObject:
                    Logger.Debug($"Instantiating object {instantiateObject.type.Name}...");
                    instantiateObjectFromAssembly(instantiateObject.type, instantiateObject.args);
                    _serverCommunication.Send(new SuccessResponse()
                    {
                        message = $"Sucessfully instantiated {instantiateObject.type.FullName} from assembly"
                    });
                    Logger.Debug("Done instantiating!");
                    break;
                case RunMethod runMethod:
                    Logger.Debug($"Running mehtod {runMethod.memberName}");
                    _serverCommunication.Send(ObjectSerializer.Serialize(runMethodFromAssembly(runMethod)));
                    Logger.Info($"Done running method {runMethod.memberName}");
                    break;
                case LoadAssembly loadAssembly:
                    Logger.Debug($"Loading assembly {loadAssembly.Path}");
                    loadAssemblyFromFile(loadAssembly);
                    _serverCommunication.Send(new SuccessResponse()
                    {
                        message = $"Successfully loaded assembly from path {_targetAssembly.FullName}"
                    });
                    Logger.Debug("Done loading assembly!");
                    break;
                case Shutdown shutdown:
                    Logger.Info("Received shutdown signal");
                    return true;
                default:
                    Logger.Warn("Received unknown task!");
                    break;
            }

            return false;
        }

        private void instantiateObjectFromAssembly(Type type, object[] args)
        {
            var typeFromAssembly =
                _targetAssembly.GetType(_targetAssembly.GetName().Name + type.Name);
            calculationObject = Activator.CreateInstance(typeFromAssembly, args);
        }

        private object runMethodFromAssembly(RunMethod runMethod)
        {
            return runMethod.type.InvokeMember(runMethod.memberName, BindingFlags.InvokeMethod, null,
                calculationObject, runMethod.args);
        }

        private void loadAssemblyFromFile(LoadAssembly loadAssembly)
        {
            _targetAssembly = Assembly.LoadFile(loadAssembly.Path);
        }
    }
}