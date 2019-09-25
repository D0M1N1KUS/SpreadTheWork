using System;
using System.Collections.Generic;

namespace Core.DataClasses
{
    [SerializableAttribute]
    public class EncapsulatedList
    {
        public List<int> list = new List<int>();
    }
}