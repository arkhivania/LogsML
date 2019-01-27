using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.Server.Base
{
    public interface IIndex<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        string Name { get; }
        long Count { get; }

        IEnumerable<(TKey, TValue)> Read();
        IEnumerable<(TKey, TValue)> ReadReversed();

        IEnumerable<(TKey, TValue)> Find(IEnumerable<TKey> keys);
        IEnumerable<(TKey, TValue)> Larger(TKey key, bool inclusive);
        IEnumerable<(TKey, TValue)> Smaller(TKey key, bool inclusive);

        void Add(TKey key, TValue value);
    }
}
