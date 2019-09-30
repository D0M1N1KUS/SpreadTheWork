using System.Collections.Generic;

namespace SampleAssembly
{
    public class Sorter
    {
        public List<int> BubbleSort(List<int> colleciton)
        {
            bool changesOcurred;

            do
            {
                changesOcurred = false;

                for (var i = 1; i < colleciton.Count; i++)
                {
                    if (colleciton[i - 1] > colleciton[i])
                    {
                        var tmp = colleciton[i];
                        colleciton[i] = colleciton[i - 1];
                        colleciton[i - 1] = tmp;
                        changesOcurred = true;
                    }
                }
            } while (changesOcurred);

            return colleciton;
        }
    }
}