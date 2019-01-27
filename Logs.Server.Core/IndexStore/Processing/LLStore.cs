using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Logs.Server.Core.Server.Base;

namespace Logs.Server.Core.IndexStore.Processing
{
    class LLStore : Base.IIndexStore<long, ulong>
    {
        private readonly Base.IndexStoreSettings settings;

        public LLStore(Base.IndexStoreSettings settings)
        {
            this.settings = settings;
            if (!Directory.Exists(settings.Folder))
                Directory.CreateDirectory(settings.Folder);
        }

        public void Load(IIndex<long, ulong> index)
        {
            var fname = Path.Combine(settings.Folder, index.Name);
            if (File.Exists(fname))
                using (var stream = new FileStream(fname, FileMode.Open, FileAccess.Read))
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                using (var br = new BinaryReader(gzip))
                {
                    var c = br.ReadInt64();
                    for(long i = 0; i < c; ++i)
                    {
                        var k = br.ReadInt64();
                        var v = br.ReadUInt64();
                        index.Add(k, v);
                    }
                }
        }

        public void Store(IIndex<long, ulong> index)
        {
            var fname = Path.Combine(settings.Folder, index.Name);

            using (var stream = new FileStream(fname, FileMode.Create, FileAccess.Write))
            using (var gzip = new GZipStream(stream, CompressionLevel.Optimal))
            using (var bw = new BinaryWriter(gzip))
            {
                bw.Write(index.Count);

                foreach (var (k, v) in index.Read())
                {
                    bw.Write(k);
                    bw.Write(v);
                }
            }
        }
    }
}
