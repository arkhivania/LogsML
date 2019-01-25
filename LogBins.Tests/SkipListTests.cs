using LogBins.Lists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    [Parallelizable]
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
        public void IntTests(int maxL, int maxV, bool inclusive)
        {
            var sl = new SkipList<int, int>();
            

            var comp = new Func<int, int, bool>((q1, q2) => q1 > q2);
            if(inclusive)
                comp = new Func<int, int, bool>((q1, q2) => q1 >= q2);

            var seq = GenerateSeqRandomLengthI(maxL, maxV);
            foreach (var s in seq)
                sl.Add(s, s);

            var thresh = (int)((random.NextDouble() - 0.5) * seq.Length * 3.0);

            var count_in_seq = seq.Where(q => comp(q, thresh)).Count();
            var count_in_skip_l = sl.Larger(thresh, inclusive).ToArray();

            Assert.That(count_in_skip_l.All(w => comp(w, thresh)));
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
            var sl = new SkipList<double, double>();
            var sourceSeq = GenerateSeqI(count, count);                

            foreach (var s in sourceSeq)
                sl.Add(s, s);

            for (int k = 0; k < 10; ++k)
            {
                var thresh = (random.NextDouble() - 0.5) * count * 3.0;

                var count_in_seq = sourceSeq.Where(q => q > thresh).Count();
                var count_in_skip_l = sl.Larger(thresh, false).ToArray();

                Assert.That(count_in_skip_l.All(w => w > thresh));
                Assert.AreEqual(count_in_seq, count_in_skip_l.Length);
            }
        }

        [Test]
        public void CheckTheSame()
        {
            var sl = new SkipList<int, int>();

            var sourceSeq = new[]
            {
                20, 20, 2, 552, 552, 45, 125, 44, 77
            };

            foreach (var s in sourceSeq)
                sl.Add(s, s);

            var larger = sl.Larger(100, false).ToArray();
            Assert.That(larger.All(q => q > 100));
            Assert.AreEqual(3, larger.Length);

            var m200 = sl.Larger(-200, false).ToArray();
            Assert.AreEqual(sourceSeq.Length, m200.Length);
        }
    }
}
