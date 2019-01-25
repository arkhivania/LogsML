using LogBins.Base;
using LogBins.ZipBuckets;
using System.Collections.Generic;
using System.IO;

namespace LogBins.Tests.Tools
{
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
}
