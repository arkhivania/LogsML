using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogBins.Structures.Lists.Storage
{
    public interface IKVSerializer<TKey, TValue>
    {
        void Write(Stream targetStream, IEnumerable<(TKey, TValue)> keyValues);
        IEnumerable<(TKey, TValue)> Read(Stream source, int count);
    }
}
