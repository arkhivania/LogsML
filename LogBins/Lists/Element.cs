using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    class Element<TKey, TValue>
        where TKey : IComparable<TKey>
    {   
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public Element<TKey, TValue> PrevElement { get; set; }
        public Element<TKey, TValue> NextElement { get; set; }
        public Element<TKey, TValue> NextLevelElement { get; set; }

        public ElementType ElementType { get; set; }
    }
}
