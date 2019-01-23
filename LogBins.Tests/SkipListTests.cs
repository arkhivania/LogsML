using LogBins.Lists;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    class SkipListTests
    {
        [Test]
        [TestCase(100)]
        [TestCase(400)]
        [TestCase(500)]
        [TestCase(800)]
        [TestCase(8000)]
        public void RandomTests(int count)
        {
            var random = new Random(count);

            var sl = new SkipList<double, double>();
            var sourceSeq = Enumerable.Range(0, count)
                .Select(q => random.Next(count))
                .ToArray();

            foreach (var s in sourceSeq)
                sl.Add(s, s);

            for (int k = 0; k < 10; ++k)
            {
                var thresh = (random.NextDouble() - 0.5) * count * 3.0;

                var count_in_seq = sourceSeq.Where(q => q > thresh).Count();
                var count_in_skip_l = sl.LargerThan(thresh).ToArray();

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

            var larger = sl.LargerThan(100).ToArray();
            Assert.That(larger.All(q => q > 100));
            Assert.AreEqual(3, larger.Length);

            var m200 = sl.LargerThan(-200).ToArray();
            Assert.AreEqual(sourceSeq.Length, m200.Length);

            //Assert.That(sl.ToArray().Length == 6);
        }
    }
}
