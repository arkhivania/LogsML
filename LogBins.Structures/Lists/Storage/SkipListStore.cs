using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogBins.Structures.Lists.Storage
{
    public class SkipListStore<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private readonly IKVSerializer<TKey, TValue> serializer;

        public SkipListStore(IKVSerializer<TKey, TValue> serializer)
        {
            this.serializer = serializer;
        }        

        IEnumerable<Element<TKey, TValue>> Line(Element<TKey, TValue> start)
        {            
            if (start.ElementType != ElementType.Head)
                throw new ArgumentException("Line must starts with head element");

            var curE = start.NextElement;

            while (curE.ElementType != ElementType.Tail)
            {
                yield return curE;
                curE = curE.NextElement;
            }
        }

        public void Store(Stream targetStream, SkipList<TKey, TValue> skipList)
        {
            var levels = new IEnumerable<Element<TKey, TValue>>[skipList.numLevels];
            var curE = skipList.leftTopItem;            
            for (int i = 0; i < levels.Length; ++i)
            {
                levels[i] = Line(curE);
                curE = curE.NextLevelElement;                
            }

            //var indices = new List<int>[levels.Length];
            //var positions = new int[levels.Length];
            //while (true)
            //{
            //    for(int l = levels.Length - 1; l >= 0; --l)
            //    {

            //    }
            //}
        }
    }
}
