using System;
using System.Collections.Generic;
using System.Threading;
using ServerAssemblyInterface.Interfaces;

namespace SampleAssembly
{
    public class Algorithm : IStartAlgorithm
    {
        private List<int> list1 = new List<int>(){9,8,7,6,5,4,3,2,1};
        private List<int> list2 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list3 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list4 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list5 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list6 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list7 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list8 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list9 = new List<int>(){9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1,9,8,7,6,5,4,3,2,1};
        private List<int> list10 = new List<int>(){9,8,7,6,5,4,3,2,1};
        
        public void Run(IRunner runner)
        {
            runner.InstantiateObject(typeof(Sorter), null);

            var taskIds = new List<long>();
            
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list1}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list2}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list3}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list4}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list5}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list6}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list7}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list8}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list9}));
            taskIds.Add(runner.RunFunction(typeof(Sorter), "BubbleSort", new object[] {list10}));
            
            while(!runner.TasksCompleted())
                Thread.Sleep(100);

            for (var i = 0; i < taskIds.Count; i++)
            {
                var sortedList = (List<int>)runner.GetResult(taskIds[i]);
                Console.WriteLine($"List {i + 1}:");
                foreach (var item in sortedList)
                {
                    Console.Write($"{item} ");
                }
                
                Console.WriteLine();
            }
        }
    }
}