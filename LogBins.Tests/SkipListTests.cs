using Maybe.SkipList;
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
        public void CheckTheSame()
        {
            var sl = new SkipList<int>();
            sl.Add(0);            
            sl.Add(2);
            sl.Add(2);
            sl.Add(4);
            sl.Add(5);
            sl.Add(0);

            Assert.That(sl.ToArray().Length == 6);
        }
    }
}
