using LogBins;
using LogBins.Base;
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

namespace MLRoots.Deduplication.Tests
{
    [TestFixture]
    class Chk
    {
        IEnumerable<string> LoadLines(string fileName)
        {
            using (var ar = ZipFile.OpenRead(fileName))
                foreach (var e in ar.Entries
                    .Where(q => q.Length > 0))
                {
                    using (var stream = e.Open())
                    using (var sr = new StreamReader(stream))
                        while (!sr.EndOfStream)
                        {
                            var log_line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(log_line))
                                yield return log_line;
                        }
                }
        }

        class SP : IBucketStreamProvider
        {
            class NCS : MemoryStream
            {
                public byte[] Data { get; private set; }

                public override void Close()
                {
                    this.Data = ToArray();
                    base.Close();
                }
            }

            readonly Dictionary<BucketAddress, NCS> streams 
                = new Dictionary<BucketAddress, NCS>();

            public Stream OpenRead(BucketAddress bucketAddress)
            {
                if (streams.TryGetValue(bucketAddress, out NCS stream))
                    return new MemoryStream(stream.Data);

                return streams[bucketAddress] = new NCS();
            }

            public Stream OpenWrite(BucketAddress bucketAddress)
            {
                return streams[bucketAddress] = new NCS();
            }
        }

        class MS : IMetaStorage
        {
            Dictionary<BagAddress, int> bagBuckets = new Dictionary<BagAddress, int>();

            public Task<int> GetCurrentBucketIndexForBag(BagAddress bagAddress)
            {
                if (bagBuckets.TryGetValue(bagAddress, out int bv))
                    return Task.FromResult(bv);

                return Task.FromResult(0);
            }

            public Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, int id)
            {
                bagBuckets[bagAddress] = id;
                return Task.CompletedTask;
            }

            public Task<BagInfo[]> LoadBags(ushort trainId)
            {
                return Task.FromResult(new BagInfo[] { });
            }

            public Task RegisterNewBag(ushort trainId, BagInfo bagInfo)
            {
                return Task.CompletedTask;
            }
        }

        [Test]
        //[TestCase(true, @"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        //[TestCase(true, @"..\..\..\..\TestsData\lgs\full.zip")]
        //[TestCase(true, @"..\..\..\..\TestsData\lgs\printer.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\full.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\printer.zip")]
        [TestCase(@"..\..\..\..\TestsData\lgs\printer_nlmode.zip")]
        public async Task Clusterization(string fileName)
        {
            var log_lines = new List<string>();
            Assert.That(File.Exists(fileName), "Input file not found");

            log_lines.AddRange(LoadLines(fileName));
            Assert.That(log_lines.Count > 0, "Log lines not empty");

            var t_b = new TrainBag(0,
                new BucketFactory(new SP()),
                new MS(),
                new BagSettings { PerBucketMessages = 5000 });

            var all_time = Stopwatch.StartNew();

            var chk_dict = new Dictionary<EntryAddress, string>();

            foreach (var m in log_lines.Take(50000))
            {
                var a = await t_b.Push(new LogEntry { Message = m });
                chk_dict[a] = m;
            }

            all_time.Stop();

            foreach(var s in chk_dict)
            {
                var entry = await t_b.ReadEntry(s.Key);
                Assert.AreEqual(entry.Message, s.Value);
            }

            await t_b.Close();

            //var all_size = t_b
            //    .OneItemBags
            //    .SelectMany(q => q.CompleteSet)
            //    .Select(q => (long)q.Message.Length).Sum();

            //var compressed = t_b
            //    .OneItemBags
            //    .SelectMany(q => q.Buckets)
            //    .Select(q => q.GetCompressed().Length)
            //    .Sum();

            //TestContext.Write(
            //    new
            //    {
            //        Compressed = compressed,
            //        GroupsCount = t_b.OneItemBags.Count,
            //        AllSize = all_size,
            //        Ratio = (float)compressed / (float)all_size,
            //        RRatio = (float)all_size / (float)compressed, 
            //        CountInSec = log_lines.Count * 1000 / all_time.ElapsedMilliseconds,
            //        AllCount = t_b.AllCount, 
            //        MaxZips = t_b.OneItemBags.Select(q => q.Buckets.Count).Max()
            //    });
        }
    }
}
