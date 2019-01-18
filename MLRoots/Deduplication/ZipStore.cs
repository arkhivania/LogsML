using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MLRoots.Deduplication
{
    class ZipStore
    {
        readonly MemoryStream compressedStream = new MemoryStream();
        readonly ICSharpCode.SharpZipLib.GZip.GZipOutputStream zip;

        public List<string> Lines { get; } = new List<string>();

        public ZipStore()
        {
            this.zip = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(compressedStream);
            zip.SetLevel(9);
        }

        public void Store(string line)
        {
            var bytes_to_write = Encoding
                .UTF8
                .GetBytes(line);

            Lines.Add(line);

            zip.Write(bytes_to_write, 0, bytes_to_write.Length);
        }

        public void Save()
        {
            zip.Dispose();
        }

        public byte[] GetCompressed()
        {
            Save();

            return compressedStream.ToArray();
        }
    }
}
