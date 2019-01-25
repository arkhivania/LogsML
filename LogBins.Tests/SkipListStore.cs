using LogBins.Structures.Lists;
using LogBins.Structures.Lists.Storage;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    class SkipListStore
    {
        readonly Random random = new Random(0);

        class Int32Serializer : IKVSerializer<int, int>
        {
            public IEnumerable<(int, int)> Read(Stream source, int count)
            {
                throw new NotImplementedException();
            }

            public void Write(Stream targetStream, IEnumerable<(int, int)> keyValues)
            {
                throw new NotImplementedException();
            }
        }


        int[] GenerateSeqI(int count, int maxValue)
        {
            return Enumerable.Range(0, count)
                .Select(q => random.Next(maxValue)).ToArray();
        }

        [Test]
        [TestCase(10)]
        public void Lst(int count)
        {
            var seq = GenerateSeqI(count, count / 2);

            var sl = new SkipList<int, int>(SkipListDistances.Int32);
            foreach (var kv in seq)
                sl.Add(kv, kv);

            var store = new SkipListStore<int, int>(new Int32Serializer());
            using (var ms = new MemoryStream())
            {
                store.Store(ms, sl);
            }
        }
    }
}
