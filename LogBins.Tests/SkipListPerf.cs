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
        public void KeyValues(int length)
        {
            var seq = GenerateSeqI(length, length);
            var sl = new SkipList<int, int>((a,b) => Math.Abs(a - b));

            foreach (var s in seq)
                sl.Add(s, s);

            var kv_seq = sl.KeyValues().ToArray();
            var r_seq = sl.KeyValuesReversed().ToArray();

            Assert.That(kv_seq.Reverse().SequenceEqual(r_seq));
        }

        [Test]
        [TestCase(100)]
        [TestCase(1090)]
        [TestCase(4190)]
        [TestCase(84190)]
        [TestCase(142345)]
        [TestCase(542345)]
        public void InsertionSpeedNearSorted(int length)
        {
            var sl = new SkipList<int, int>((a, b) => Math.Abs(a - b));

            var r = new Random();
            for (int i = 0; i < length; ++i)
            {
                var sk = random.Next(50);
                sl.Add(i - sk, i);
            }

            var kv_seq = sl.KeyValues().ToArray();
            var r_seq = sl.KeyValuesReversed().ToArray();

            Assert.That(kv_seq.Reverse().SequenceEqual(r_seq));
        }
    }
}
