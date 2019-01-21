using LogBins.Base;
using LogBins.ZipBuckets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace LogBins.Import
{
    class Program
    {
        static IEnumerable<string> LoadLines(string fileName)
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
            Dictionary<BagAddress, uint> bagBuckets = new Dictionary<BagAddress, uint>();

            public Task<uint> GetCurrentBucketIndexForBag(BagAddress bagAddress)
            {
                if (bagBuckets.TryGetValue(bagAddress, out uint bv))
                    return Task.FromResult(bv);

                return Task.FromResult(0u);
            }

            public Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, uint id)
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

        class ERCompare : IComparer<ER>
        {
            public int Compare(ER x, ER y)
            {
                var xea = x.EntryAddress;
                var yea = y.EntryAddress;

                ulong xa = ((ulong)xea.TrainId << 48) + ((ulong)xea.BagId << 16) + (ulong)xea.Index;
                ulong ya = ((ulong)yea.TrainId << 48) + ((ulong)yea.BagId << 16) + (ulong)yea.Index;

                return xa.CompareTo(ya);
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

        static async Task Main(string[] args)
        {
            var file = @"..\..\..\..\TestsData\lgs\syslog_short.zip";

            var t_b = new TrainBag(0,
                new BucketFactory(new SP()),
                new MS(),
                new BagSettings { PerBucketMessages = 5000 });

            var clist = new Maybe.SkipList.SkipList<ER>(new ERCompare());
            //var chk_dict = new Dictionary<EntryAddress, string>();

            int index = 0;
            foreach (var l in LoadLines(file))
            {
                var addr = await t_b.Push(new LogEntry { Message = l });
                clist.Add(new ER(addr, l));
                //chk_dict[addr] = l;


                if ((++index) % 1000 == 0)
                {
                    Console.WriteLine($"{addr.Index}");
                    Console.WriteLine($"{index} messages");
                }
            }

            index = 0;
            Console.WriteLine("Reading ...");
            foreach (var s in clist)
            {   
                var entry = await t_b.ReadEntry(s.EntryAddress);
                if (entry.Message != s.Message)
                    throw new InvalidOperationException("Read write error");

                if ((index++) % 1000 == 0)
                    Console.WriteLine($"{index} messages");
            }
        }
    }
}
