using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    public class SkipList<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        readonly Element<TKey, TValue> leftTopItem;
        private readonly int numLevels;

        readonly Random random = new Random(0);

        public SkipList(int numLevels = 4)
        {
            this.numLevels = numLevels;
            if (numLevels <= 1)
                throw new ArgumentException("numLevels <= 1");

            var heads = new Element<TKey, TValue>[numLevels];

            for (int i = 0; i < numLevels; ++i)
            {
                var tail = new Element<TKey, TValue>() { IsTail = true };
                heads[i] = new Element<TKey, TValue>() { NextElement = tail };
            }

            for (int i = 0; i < numLevels - 1; ++i)
                heads[i].NextLevelElement = heads[i + 1];

            leftTopItem = heads[0];
        }

        bool IsLessOrEqual(TKey key, Element<TKey, TValue> item)
        {
            if (item.IsTail)
                return true;

            return key.CompareTo(item.Key) <= 0;
        }

        public IEnumerable<TValue> LargerThan(TKey key)
        {
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (!IsLessOrEqual(key, curE.NextElement))
                    curE = curE.NextElement;

                if (level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            if (curE.IsTail)
                yield break;

            curE = curE.NextElement;

            while (!curE.IsTail)
            {
                if(key.CompareTo(curE.Key) < 0)
                    yield return curE.Value;

                curE = curE.NextElement;
            }
        }

        public void Add(TKey key, TValue value)
        {
            var newElement = new Element<TKey, TValue>() { Key = key, Value = value };
            var levels = new Element<TKey, TValue>[numLevels];
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (!IsLessOrEqual(key, curE.NextElement))
                    curE = curE.NextElement;

                levels[level] = curE;
                if(level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            newElement.NextElement = curE.NextElement;
            curE.NextElement = newElement;

            curE = newElement;
            level = numLevels - 1;
            while(level > 0 && random.Next(2) == 1)
            {
                level--;
                var lel = new Element<TKey, TValue>()
                {
                    Key = key,
                    Value = value,
                    NextLevelElement = curE,
                    NextElement = levels[level].NextElement
                };

                levels[level].NextElement = lel;                
                curE = lel;
            }
        }
    }
}
