using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MLRoots.Deduplication
{
    class OneItemBag
    {
        const int TrainCount = 25;
        const int MessagesInBag = 1000;

        public int CompleteCount => ZipStores.Select(q => q.Lines.Count).Sum();
        public List<ZipStore> ZipStores { get; } = new List<ZipStore>();

        public IEnumerable<string> CompleteSet => ZipStores.SelectMany(q => q.Lines);

        public List<string> TrainSet { get; } = new List<string>();

        public string BaseMessage { get; }
        public uint Label { get; }

        readonly Random random = new Random();        

        public OneItemBag(string baseMessage, uint label)
        {
            this.BaseMessage = baseMessage;
            this.Label = label;            

            AddMessage(baseMessage, true);
        }

        public void AddMessage(string message, bool forceAddToTrain = false)
        {
            if (ZipStores.Count == 0
                || ZipStores[ZipStores.Count - 1].Lines.Count >= MessagesInBag)
                ZipStores.Add(new ZipStore());

            ZipStores[ZipStores.Count - 1].Store(message);

            if (forceAddToTrain || TrainSet.Count < TrainCount)
                if (forceAddToTrain || random.Next(5) == 0)
                    TrainSet.Add(message);
        }
    }
}
