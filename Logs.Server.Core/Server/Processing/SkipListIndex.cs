using LogBins.Structures.Lists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logs.Server.Core.Server.Processing
{
    class SkipListIndex<TKey, TValue> : Base.IIndex<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        readonly SkipList<TKey, TValue> skipList;

        public string Name { get; }
        public long Count => skipList.Count;

        public SkipListIndex(string name, 
            Func<TKey, TKey, float> distanceFunc)
        {
            this.skipList = new SkipList<TKey, TValue>(distanceFunc);
            this.Name = name;
        }

        public void Add(TKey key, TValue value)
        {
            skipList.Add(key, value);
        }

        public IEnumerable<(TKey, TValue)> Find(IEnumerable<TKey> keys)
        {
            foreach (var k in keys)
                foreach (var v in skipList.Larger(k, true)
                    .Where(q => q.Key.CompareTo(k) == 0))
                    yield return (v.Key, v.Value);
        }

        public IEnumerable<(TKey, TValue)> Smaller(TKey key, bool inclusive)
        {
            foreach (var v in skipList.Smaller(key, inclusive))
                yield return (v.Key, v.Value);
        }

        public IEnumerable<(TKey, TValue)> Larger(TKey key, bool inclusive)
        {
                foreach (var v in skipList.Larger(key, inclusive))
                    yield return (v.Key, v.Value);
        }

        public IEnumerable<(TKey, TValue)> Read()
        {
            foreach (var v in skipList.KeyValues())
                yield return (v.Key, v.Value);
        }

        public IEnumerable<(TKey, TValue)> ReadReversed()
        {
            foreach (var v in skipList.KeyValuesReversed())
                yield return (v.Key, v.Value);
        }
    }
}
