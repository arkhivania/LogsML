using LogBins;
using LogBins.Base;
using LogBins.Simple;
using LogBins.Tests.Tools;
using LogBins.ZipBuckets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Tests
{
    [TestFixture]
    partial class Chk
    {

        [Test]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\full.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\printer.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\printer_nlmode.zip")]
        public async Task Clusterization(string fileName)
        {
            var sp = new SP();
            var ms = new MS();

            var clist = new List<ER>();

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

                    if ((++index) % 100000 == 0)
                        TestContext.Progress.WriteLine($"{index} messages");
                }
                pushSW.Stop();

                TestContext.WriteLine($"Push time: {pushSW.ElapsedMilliseconds} ms, Speed: {(index * 1000)/(pushSW.ElapsedMilliseconds + 1)} msgs/sec");
            }

            using (var t_b = new TrainBag(1,
                new BucketFactory(new ZipStoreFactory(sp)),
                ms,
                new BagSettings { PerBucketMessages = 5000 }))
            {
                var index = 0;

                TestContext.Progress.WriteLine("Reading ...");
                foreach (var s in clist)
                {
                    var entry = await t_b.ReadEntry(s.EntryAddress);
                    if (entry.Message != s.Message)
                        throw new InvalidOperationException("Read write error");

                    if ((++index) % 100000 == 0)
                        TestContext.Progress.WriteLine($"{index} messages");
                }
            }

            var compressedSize = sp.streams
                .Select(q => q.Value).Select(q => q.Data.Length).Sum();
            var sourceSize = new FileInfo(fileName).Length;
            TestContext.WriteLine($"Compression: {compressedSize} vs {sourceSize} ({(compressedSize * 100) / sourceSize})");
        }
    }
}
