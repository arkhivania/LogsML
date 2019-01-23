using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    class Element<TKey>
        where TKey : IComparable<TKey>
    {
        public bool IsTail { get; set; }
        public TKey Key { get; set; }

        public Element<TKey> NextElement { get; set; }
        public Element<TKey> NextLevelElement { get; set; }
    }
}
