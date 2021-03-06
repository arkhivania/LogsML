﻿using LogBins.Structures.Lists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    [Parallelizable]
    [Category("PERF")]
    class SkipListTests
    {
        readonly Random random = new Random(0);

        int[] GenerateSeqRandomLengthI(int maxCount, int maxValue)
        {            
            return Enumerable.Range(0, random.Next(maxCount))
                .Select(q => random.Next(maxValue)).ToArray();
        }

        int[] GenerateSeqI(int count, int maxValue)
        {
            return Enumerable.Range(0, count)
                .Select(q => random.Next(maxValue)).ToArray();
        }

        

        [Test]
        [TestCase(1, 10, true)]
        [TestCase(2, 10, true)]
        [TestCase(100, 10, true)]
        [TestCase(10000, 100, true)]
        [TestCase(1, 10, false)]
        [TestCase(2, 10, false)]
        [TestCase(100, 10, false)]
        [TestCase(10000, 100, false)]        
        [Repeat(10)]
        public void IntTestsLarger(int maxL, int maxV, bool inclusive)
        {
            var sl = new SkipList<int, int>((a, b) => Math.Abs(a - b));            

            var comp_thr = 0;
            if(inclusive)
                comp_thr = -1;

            var seq = GenerateSeqRandomLengthI(maxL, maxV);
            foreach (var s in seq)
                sl.Add(s, s);

            var thresh = (int)((random.NextDouble() - 0.5) * seq.Length * 3.0);

            var count_in_seq = seq.Where(q => q.CompareTo(thresh) > comp_thr).Count();
            var count_in_skip_l = sl.Larger(thresh, inclusive).ToArray();

            Assert.That(count_in_skip_l.All(w => w.Key.CompareTo(thresh) > comp_thr));
            Assert.AreEqual(count_in_seq, count_in_skip_l.Length);
        }

        [Test]
        [TestCase(1, 10, true)]
        [TestCase(2, 10, true)]
        [TestCase(100, 10, true)]
        [TestCase(10000, 100, true)]
        [TestCase(1, 10, false)]
        [TestCase(2, 10, false)]
        [TestCase(100, 10, false)]
        [TestCase(10000, 100, false)]
        [Repeat(10)]
        public void IntTestsSmaller(int maxL, int maxV, bool inclusive)
        {
            var sl = new SkipList<int, int>((a, b) => Math.Abs(a - b));

            var comp_thr = 0;
            if (inclusive)
                comp_thr = 1;

            var seq = GenerateSeqRandomLengthI(maxL, maxV);
            foreach (var s in seq)
                sl.Add(s, s);

            var thresh = (int)((random.NextDouble() - 0.5) * seq.Length * 3.0);

            var count_in_seq = seq.Where(q => q.CompareTo(thresh) < comp_thr).Count();
            var count_in_skip_l = sl.Smaller(thresh, inclusive).ToArray();

            Assert.That(count_in_skip_l.All(w => w.Key.CompareTo(thresh) < comp_thr));
            Assert.AreEqual(count_in_seq, count_in_skip_l.Length);
        }

        [Test]
        [TestCase(100)]
        [TestCase(400)]
        [TestCase(500)]
        [TestCase(800)]
        [TestCase(8000)]
        public void LargerTests(int count)
        {
            var sl = new SkipList<double, double>((a, b) => (float)Math.Abs(a - b));
            var sourceSeq = GenerateSeqI(count, count);                

            foreach (var s in sourceSeq)
                sl.Add(s, s);

            for (int k = 0; k < 10; ++k)
            {
                var thresh = (random.NextDouble() - 0.5) * count * 3.0;

                var count_in_seq = sourceSeq.Where(q => q > thresh).Count();
                var count_in_skip_l = sl.Larger(thresh, false).ToArray();

                Assert.That(count_in_skip_l.All(w => w.Key > thresh));
                Assert.AreEqual(count_in_seq, count_in_skip_l.Length);
            }
        }

        [Test]
        public void CheckTheSame()
        {
            var sl = new SkipList<int, int>((a, b) => Math.Abs(a - b));

            var sourceSeq = new[]
            {
                20, 20, 2, 552, 552, 45, 125, 44, 77
            };

            foreach (var s in sourceSeq)
                sl.Add(s, s);

            var larger = sl.Larger(100, false).ToArray();
            Assert.That(larger.All(q => q.Key > 100));
            Assert.AreEqual(3, larger.Length);

            var m200 = sl.Larger(-200, false).ToArray();
            Assert.AreEqual(sourceSeq.Length, m200.Length);


            var smaller = sl.Smaller(100, false).ToArray();
            Assert.That(smaller.All(q => q.Key < 100));
            Assert.AreEqual(6, smaller.Length);

            var l200 = sl.Smaller(1200, false).ToArray();
            Assert.AreEqual(sourceSeq.Length, l200.Length);
        }
    }
}
