﻿using LogBins.Base;
using LogBins.Lists;
using LogBins.Simple;
using LogBins.Tests.Tools;
using LogBins.ZipBuckets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Tests
{
    [TestFixture]
    class IndexBuild
    {
        [Test]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        public async Task PrimaryIndex(string fileName)
        {
            var random = new Random(0);

            var sp = new SP();
            var ms = new MS();

            var clist = new List<ER>();

            var dateTime = DateTime.Now;

            var dateIndex = new SkipList<DateTime, ulong>((a, b) => (float)System.Math.Abs(a.Subtract(b).TotalSeconds));

            using (var t_b = new TrainBag(1,
                new BucketFactory(new ZipStoreFactory(sp)),
                ms,
                new BagSettings { PerBucketMessages = 5000 }))
            {
                int index = 0;
                var pushSW = Stopwatch.StartNew();
                foreach (var l in ZipLogs.LoadLines(fileName))
                {
                    var addr = await t_b.Push(new LogEntry { Message = l });
                    clist.Add(new ER(addr, l));

                    dateIndex.Add(dateTime, addr);
                    dateTime += TimeSpan.FromSeconds((random.NextDouble() - 0.2) * 20.0);

                    if ((++index) % 100000 == 0)
                        TestContext.Progress.WriteLine($"{index} messages");
                }
                pushSW.Stop();

                TestContext.WriteLine($"Push time: {pushSW.ElapsedMilliseconds} ms, Speed: {(index * 1000) / (pushSW.ElapsedMilliseconds + 1)} msgs/sec");
            }
        }
    }
}
