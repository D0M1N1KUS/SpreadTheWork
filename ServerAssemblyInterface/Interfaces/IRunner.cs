using System;
using System.Threading.Tasks;

namespace ServerAssemblyInterface.Interfaces
{
    public interface IRunner
    {
        void LoadAssembly(string path);
        void InstantiateObject(Type type, object[] args);
        long RunFunction(Type objectType, string memberName, object[] args);
        object GetResult(long taskId);
        bool TasksCompleted();
        void Finish();
    }
}