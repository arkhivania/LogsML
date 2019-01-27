using LogBins.Base;
using LogBins.Simple;
using LogBins.Structures.Lists;
using LogBins.Tests.Tools;
using LogBins.ZipBuckets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            var startDateTime = DateTime.Now;
            var dateTime = startDateTime;

            var dt_start = DateTime.Now;
            var st_ticks = new DateTime(2018, 1, 1).Ticks;
            var dt_st_ticks = dt_start.Ticks;
            var delta = dt_st_ticks - st_ticks;

            var dateIndex = new SkipList<long, ulong>((a, b) => (float)Math.Abs(a - b));

            using (var t_b = new TrainBag(1,
                new BucketFactory(new ZipStoreFactory(sp)),
                ms,
                new BagSettings { PerBucketMessages = 5000 }))
            {
                int index = 0;

                for (int k = 0; k < 10; ++k)
                {
                    var pushSW = Stopwatch.StartNew();

                    foreach (var l in ZipLogs.LoadLines(fileName))
                    {
                        var addr = await t_b.Push(l);
                        clist.Add(new ER(addr, l));

                        dateIndex.Add(dateTime.Ticks, addr);
                        dateTime += TimeSpan.FromSeconds((random.NextDouble() - 0.2) * 20.0);

                        if ((++index) % 100000 == 0)
                            TestContext.Progress.WriteLine($"{index} messages");
                    }
                    pushSW.Stop();
                    TestContext.Progress.WriteLine($"Iteration: {k}, time = {pushSW.ElapsedMilliseconds} ms");
                }

                var lrg_d = startDateTime + TimeSpan.FromSeconds((dateTime - startDateTime).TotalSeconds / 2);
                var kv_s = dateIndex
                    .Larger(lrg_d.Ticks, false)
                    .Take(10).ToArray();                

                var rebT = Stopwatch.StartNew();

                var dateIndex2 = new SkipList<long, ulong>((a, b) => (float)Math.Abs(a - b));
                foreach (var a in dateIndex.KeyValues())
                    dateIndex2.Add(a.Key, a.Value);
                rebT.Stop();

                TestContext.Progress.WriteLine($"Rebuild time: {rebT.ElapsedMilliseconds} ms");
                //TestContext.WriteLine($"Push time: {pushSW.ElapsedMilliseconds} ms, Speed: {(index * 1000) / (pushSW.ElapsedMilliseconds + 1)} msgs/sec");
            }
        }
    }
}
