using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    class ZipStore : LogBins.Base.IBucketStore
    {
        private readonly BucketAddress bucketInfo;

        public ZipStore(BucketAddress bucketInfo)
        {
            this.bucketInfo = bucketInfo;
        }

        public IEnumerable<LogEntry> LoadEntries()
        {
            yield break;
        }

        public void StoreEntries(IEnumerable<LogEntry> entries)
        {
            using (var compressedStream = new MemoryStream())
            using (var zip = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(compressedStream))
            using (var bw = new BinaryWriter(zip))
            {
                zip.SetLevel(9);

                foreach (var e in entries)
                {
                    var bytes = Encoding.UTF8.GetBytes(e.Message);
                    bw.Write((int)bytes.Length);
                    zip.Write(bytes, 0, bytes.Length);
                }
            }
            
        }
    }
}
