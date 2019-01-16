using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MLRoots.Deduplication
{
    class OneItemBag
    {
        const int TrainCount = 25;

        public int CompleteCount => CompleteSet.Count;

        public List<string> CompleteSet { get; } = new List<string>();
        public List<string> TrainSet { get; } = new List<string>();

        public string BaseMessage { get; }
        public uint Label { get; }

        readonly Random random = new Random();

        public OneItemBag(string baseMessage, uint label)
        {
            this.BaseMessage = baseMessage;
            this.Label = label;

            CompleteSet.Add(baseMessage);
            TrainSet.Add(baseMessage);
        }

        public void AddMessage(string message)
        {
            CompleteSet.Add(message);

            if (TrainSet.Count < TrainCount)
                if (random.Next(5) == 0)
                    TrainSet.Add(message);
        }

        public byte[] GetCompressed()
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(ms))
                {
                    zip.SetLevel(9);

                    //                using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
                    using (var swriter = new StreamWriter(zip, Encoding.UTF8))
                        foreach (var l in CompleteSet)
                            swriter.WriteLine(l);
                }
                return ms.ToArray();

            }
        }
    }
}
