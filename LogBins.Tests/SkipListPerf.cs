using LogBins.Lists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    class SkipListPerf
    {
        readonly Random random = new Random(0);

        int[] GenerateSeqI(int count, int maxValue)
        {
            return Enumerable.Range(0, count)
                .Select(q => random.Next(maxValue)).ToArray();
        }

        [Test]
        [TestCase(100)]
        [TestCase(1090)]
        [TestCase(4190)]
        [TestCase(142345)]
        public void KeyValues(int length)
        {
            var seq = GenerateSeqI(length, length);
            var sl = new SkipList<int, int>();

            foreach (var s in seq)
                sl.Add(s, s);

            
            //var kv = sl.KeyValues().ToArray();
        }
    }
}
