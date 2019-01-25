using LogBins.Lists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    [Category("PERF")]
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
        [TestCase(84190)]
        //[TestCase(142345)]
        public void KeyValues(int length)
        {
            var seq = GenerateSeqI(length, length);
            var sl = new SkipList<int, int>();

            foreach (var s in seq)
                sl.AddBD(s, s, (a,b) => Math.Abs((float)(a - b)));

            var kv_seq = sl.KeyValues().ToArray();
            var r_seq = sl.KeyValuesReversed().ToArray();

            Assert.That(kv_seq.Reverse().SequenceEqual(r_seq));
        }
    }
}
