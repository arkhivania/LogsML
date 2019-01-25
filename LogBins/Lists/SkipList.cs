using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    public class SkipList<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        readonly Element<TKey, TValue> leftTopItem;
        readonly Element<TKey, TValue> rightTopItem;

        private readonly int numLevels;

        readonly Random random = new Random(0);

        public SkipList(int numLevels = 4)
        {
            this.numLevels = numLevels;
            if (numLevels <= 1)
                throw new ArgumentException("numLevels <= 1");

            var heads = new Element<TKey, TValue>[numLevels];
            var tails = new Element<TKey, TValue>[numLevels];

            for (int i = 0; i < numLevels; ++i)
            {
                var tail = new Element<TKey, TValue>() { ElementType = ElementType.Tail };

                tails[i] = tail;
                heads[i] = new Element<TKey, TValue>() { NextElement = tail, ElementType = ElementType.Head };
            }

            for (int i = 0; i < numLevels - 1; ++i)
            {
                heads[i].NextLevelElement = heads[i + 1];
                tails[i].NextLevelElement = tails[i + 1];
            }

            leftTopItem = heads[0];
            rightTopItem = tails[0];
        }


        /// <summary>
        /// метод возвращает последовательность элементов, которая заведомо включает в себя ключи большие key
        /// однако в этой последовательности могут быть и элементы с меньшими Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<Element<TKey, TValue>> Right(TKey key)
        {
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (curE.NextElement.ElementType != ElementType.Tail
                    && key.CompareTo(curE.NextElement.Key) > 0)
                    curE = curE.NextElement;

                if (level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            if (curE.ElementType == ElementType.Tail)
                yield break;

            if (curE.ElementType == ElementType.Head)
                curE = curE.NextElement;

            while (curE.ElementType != ElementType.Tail)
            {
                yield return curE;
                curE = curE.NextElement;
            }
        }

        public IEnumerable<TValue> Larger(TKey key, bool inclusive)
        {
            var c = 0;
            if (inclusive)
                c = -1;

            foreach (var r in Right(key))
                if (r.Key.CompareTo(key) > c)
                    yield return r.Value;
        }

        public void Add(TKey key, TValue value)
        {
            var newElement = new Element<TKey, TValue>() { Key = key, Value = value };
            var levels = new Element<TKey, TValue>[numLevels];
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (curE.NextElement.ElementType != ElementType.Tail 
                    && key.CompareTo(curE.NextElement.Key) > 0)
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

        public IEnumerable<KeyValuePair<TKey, TValue>> KeyValues()
        {
            var e = leftTopItem;
            for (int i = 0; i < numLevels - 1; ++i)
                e = e.NextLevelElement;

            e = e.NextElement;
            while (e.ElementType != ElementType.Tail)
            {
                yield return new KeyValuePair<TKey, TValue>(e.Key, e.Value);
                e = e.NextElement;
            }

        }
    }
}
