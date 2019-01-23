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
        public void CheckTheSame()
        {
            var sl = new SkipList<int>();
            sl.Add(20);            
            sl.Add(2);
            sl.Add(552);
            sl.Add(45);
            sl.Add(125);
            sl.Add(44);
            sl.Add(77);

            //Assert.That(sl.ToArray().Length == 6);
        }
    }
}
