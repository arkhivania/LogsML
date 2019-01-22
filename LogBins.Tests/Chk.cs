using LogBins;
using LogBins.Base;
using LogBins.Simple;
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
            internal class NCS : MemoryStream
            {
                public byte[] Data { get; private set; }

                public override void Close()
                {
                    this.Data = ToArray();
                    base.Close();
                }
            }

            readonly internal Dictionary<BucketAddress, NCS> streams
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
            readonly List<BagInfo> bagInfos = new List<BagInfo>();
            readonly Dictionary<BagAddress, int> bagBuckets = new Dictionary<BagAddress, int>();

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
                return Task.FromResult(bagInfos.ToArray());
            }

            public Task RegisterNewBag(ushort trainId, BagInfo bagInfo)
            {
                bagInfos.Add(bagInfo);
                return Task.CompletedTask;
            }
        }

        class ER
        {
            public ER(EntryAddress entryAddress, string message)
            {
                EntryAddress = entryAddress;
                Message = message;
            }

            public EntryAddress EntryAddress { get; }
            public string Message { get; }
        }

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

            using (var t_b = new TrainBag(0,
                new BucketFactory(new ZipStoreFactory(sp)),
                ms,
                new BagSettings { PerBucketMessages = 5000 }))
            {
                int index = 0;
                foreach (var l in LoadLines(fileName))
                {
                    var addr = await t_b.Push(new LogEntry { Message = l });
                    clist.Add(new ER(addr, l));

                    if ((++index) % 100000 == 0)
                        TestContext.Progress.WriteLine($"{index} messages");
                }
            }

            using (var t_b = new TrainBag(0,
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
