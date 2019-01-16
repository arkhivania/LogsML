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

        private readonly string baseMessage;
        private readonly uint label;

        readonly List<string> trainSet = new List<string>();
        public List<string> CompleteSet => completeSet;
        readonly List<string> completeSet = new List<string>();

        public int CompleteCount => CompleteSet.Count;

        public List<string> TrainSet => trainSet;

        public string BaseMessage => baseMessage;

        public uint Label => label;

        

        readonly Random random = new Random();

        public OneItemBag(string baseMessage, uint label)
        {
            this.baseMessage = baseMessage;
            completeSet.Add(baseMessage);
            trainSet.Add(baseMessage);

            this.label = label;
        }

        public void AddMessage(string message)
        {
            CompleteSet.Add(message);

            if (trainSet.Count < TrainCount 
                || trainSet.Count == 0)
                if (random.Next(2) == 1)
                    trainSet.Add(message);
        }

        public byte[] GetCompressed()
        {
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
                using (var swriter = new StreamWriter(gzip, Encoding.UTF8))
                    foreach (var l in completeSet)
                        swriter.WriteLine(l);
                return ms.ToArray();
            }
        }
    }
}
