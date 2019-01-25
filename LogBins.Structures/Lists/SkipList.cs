using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Structures.Lists
{
    public class SkipList<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        readonly Element<TKey, TValue> leftTopItem;
        readonly Element<TKey, TValue> rightTopItem;

        private readonly int numLevels;
        private readonly Func<TKey, TKey, float> distanceFunc;
        readonly Random random = new Random(0);

        public SkipList(Func<TKey, TKey, float> distanceFunc, 
            int numLevels = 4)
        {
            this.numLevels = numLevels;
            this.distanceFunc = distanceFunc;
            if (numLevels <= 1)
                throw new ArgumentException("numLevels <= 1");

            var heads = new Element<TKey, TValue>[numLevels];
            var tails = new Element<TKey, TValue>[numLevels];

            for (int i = 0; i < numLevels; ++i)
            {
                var tail = new Element<TKey, TValue>() { ElementType = ElementType.Tail };

                tails[i] = tail;
                heads[i] = new Element<TKey, TValue>() { NextElement = tail, ElementType = ElementType.Head };
                tail.PrevElement = heads[i];
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
        /// метод возвращает последовательность элементов, которая заведомо включает в себя ключи меньше key
        /// однако в этой последовательности могут быть и элементы с большими Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<Element<TKey, TValue>> Left(TKey key)
        {
            Element<TKey, TValue> curE;
            if (LeftIsNear(key))
                curE = SearchForAddLeft(key).Item1;
            else
                curE = SearchForAddRight(key).Item1;

            while (curE.NextElement.ElementType == ElementType.Common
                && curE.NextElement.Key.CompareTo(key) == 0)                
                curE = curE.NextElement;

            while (curE.ElementType != ElementType.Head)
            {
                yield return curE;
                curE = curE.PrevElement;
            }
        }

        /// <summary>
        /// метод возвращает последовательность элементов, которая заведомо включает в себя ключи большие key
        /// однако в этой последовательности могут быть и элементы с меньшими Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<Element<TKey, TValue>> Right(TKey key)
        {
            Element<TKey, TValue> curE;
            if (LeftIsNear(key))
                curE = SearchForAddLeft(key).Item1;
            else
                curE = SearchForAddRight(key).Item1;                

            if (curE.ElementType == ElementType.Head)
                curE = curE.NextElement;

            while (curE.ElementType != ElementType.Tail)
            {
                yield return curE;
                curE = curE.NextElement;
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Smaller(TKey key, bool inclusive)
        {
            var c = 0;
            if (inclusive)
                c = 1;

            foreach (var r in Left(key))
                if (r.Key.CompareTo(key) < c)
                    yield return new KeyValuePair<TKey, TValue>(r.Key, r.Value);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Larger(TKey key, bool inclusive)
        {
            var c = 0;
            if (inclusive)
                c = -1;

            foreach (var r in Right(key))
                if (r.Key.CompareTo(key) > c)
                    yield return new KeyValuePair<TKey, TValue>(r.Key, r.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>всегда возвращает такой элемент, Next элемент которого больше, чем key</returns>
        (Element<TKey, TValue>, Element<TKey, TValue>[]) SearchForAddLeft(TKey key)
        {
            var levels = new Element<TKey, TValue>[numLevels];
            var curE = leftTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (curE.NextElement.ElementType != ElementType.Tail
                    && key.CompareTo(curE.NextElement.Key) > 0)
                    curE = curE.NextElement;

                levels[level] = curE;
                if (level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            return (curE, levels);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>всегда возвращает такой элемент, Next элемент которого больше, чем key</returns>
        (Element<TKey, TValue>, Element<TKey, TValue>[]) SearchForAddRight(TKey key)
        {
            var levels = new Element<TKey, TValue>[numLevels];
            var curE = rightTopItem;

            int level = 0;
            for (; level < numLevels; ++level)
            {
                while (curE.PrevElement.ElementType != ElementType.Head
                    && key.CompareTo(curE.PrevElement.Key) <= 0)
                    curE = curE.PrevElement;

                levels[level] = curE.PrevElement;
                if (level != numLevels - 1)
                    curE = curE.NextLevelElement;
            }

            return (curE.PrevElement, levels);
        }

        void Add(TKey key, TValue value, Func<TKey, (Element<TKey, TValue>, Element<TKey, TValue>[])> searchFunc)
        {
            var (curE, levels) = searchFunc(key);

            var newElement = new Element<TKey, TValue>() { Key = key, Value = value };

            newElement.PrevElement = curE;
            newElement.NextElement = curE.NextElement;
            curE.NextElement.PrevElement = newElement;
            curE.NextElement = newElement;

            curE = newElement;
            var level = numLevels - 1;
            while (level > 0 && random.Next(2) == 1)
            {
                level--;

                var lel = new Element<TKey, TValue>()
                {
                    Key = key,
                    Value = value,
                    NextLevelElement = curE,
                    NextElement = levels[level].NextElement,
                    PrevElement = levels[level]
                };

                levels[level].NextElement.PrevElement = lel;
                levels[level].NextElement = lel;
                curE = lel;
            }
        }

        bool LeftIsNear(TKey key)
        {
            if (KeyValues().Any() == false)
                return true;

            var left = KeyValues().First().Key;
            var right = KeyValuesReversed().First().Key;

            return distanceFunc(key, left) < distanceFunc(key, right);
        }

        public void Add(TKey key, TValue value)
        {
            if(LeftIsNear(key))
                Add(key, value, SearchForAddLeft);
            else
                Add(key, value, SearchForAddRight);
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

        public IEnumerable<KeyValuePair<TKey, TValue>> KeyValuesReversed()
        {
            var e = rightTopItem;
            for (int i = 0; i < numLevels - 1; ++i)
                e = e.NextLevelElement;

            e = e.PrevElement;
            while (e.ElementType != ElementType.Head)
            {
                yield return new KeyValuePair<TKey, TValue>(e.Key, e.Value);
                e = e.PrevElement;
            }
        }
    }
}
