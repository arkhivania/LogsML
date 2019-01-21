﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    class ZipStore : IBucketStore
    {
        private readonly BucketAddress address;
        private readonly IBucketStreamProvider bucketStreamProvider;

        public ZipStore(BucketAddress address, IBucketStreamProvider bucketStreamProvider)
        {
            this.address = address;
            this.bucketStreamProvider = bucketStreamProvider;
        }

        public IEnumerable<LogEntry> LoadEntries()
        {
            byte[] buff = new byte[256];
            using (var compressedStream = bucketStreamProvider.OpenRead(address))
            {
                if(compressedStream == null || compressedStream.Length == 0)
                    yield break;

                using (var zip = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(compressedStream))
                using (var breader = new BinaryReader(zip))
                {
                    while (zip.Position != zip.Length)
                    {
                        int cnt = breader.ReadInt32();
                        if (buff.Length < cnt)
                            buff = new byte[buff.Length + 1024];
                        int readed = 0;
                        while (readed != cnt)
                            readed += zip.Read(buff, readed, cnt - readed);

                        string message;
                        message = Encoding.UTF8.GetString(buff, 0, cnt);

                        yield return new LogEntry { Message = message };
                    }
                }
            }
        }

        public void StoreEntries(IEnumerable<LogEntry> entries)
        {
            using (var compressedStream = bucketStreamProvider.OpenWrite(address))
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
