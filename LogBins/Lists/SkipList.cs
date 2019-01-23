using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    public class SkipList<TKey>
        where TKey : IComparable<TKey>
    {
        readonly Element<TKey> leftTopItem;
        private readonly int numLevels;

        readonly Random random = new Random(0);

        public SkipList(int numLevels = 4)
        {
            this.numLevels = numLevels;
            if (numLevels <= 1)
                throw new ArgumentException("numLevels <= 1");

            var heads = new Element<TKey>[numLevels];

            for (int i = 0; i < numLevels; ++i)
            {
                var tail = new Element<TKey>() { IsTail = true };
                heads[i] = new Element<TKey>() { NextElement = tail };
            }

            for (int i = 0; i < numLevels - 1; ++i)
                heads[i].NextLevelElement = heads[i + 1];

            leftTopItem = heads[0];
        }

        bool IsLessOrEqual(Element<TKey> key, Element<TKey> item)
        {
            if (item.IsTail)
                return true;

            return key.Key.CompareTo(item.Key) <= 0;
        }

        public void Add(TKey key)
        {
            var newElement = new Element<TKey>() { Key = key };
            var levels = new Element<TKey>[numLevels];
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (!IsLessOrEqual(newElement, curE.NextElement))
                    curE = curE.NextElement;

                levels[level] = curE;
                if(level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            newElement.NextElement = curE.NextElement;
            curE.NextElement = newElement;

            curE = newElement;
            level = 3;
            while(level > 0 && random.Next(2) == 1)
            {
                level--;
                var lel = new Element<TKey>()
                {
                    Key = key,
                    NextLevelElement = curE,
                    NextElement = levels[level].NextElement
                };

                levels[level].NextElement = lel;                
                curE = lel;
            }
        }
    }
}
