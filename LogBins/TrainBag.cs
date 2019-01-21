using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using LogBins.Base;

namespace LogBins
{
    class TrainBag
    {
        public long AllCount { get; private set; } = 0;
        const double LThres = 0.85;
        private readonly Base.IBucketFactory bucketFactory;
        private readonly IMetaStorage metaStorage;
        private readonly BagSettings bagSettings;
        readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        public short TrainId { get; }

        int[] costsTempBuffer = new int[100];

        public List<Bag> Bags { get; } = new List<Bag>();

        bool initialized = false;

        public TrainBag(short trainId,
            IBucketFactory bucketFactory,
            IMetaStorage metaStorage,
            BagSettings bagSettings)
        {
            TrainId = trainId;
            this.bucketFactory = bucketFactory;
            this.metaStorage = metaStorage;
            this.bagSettings = bagSettings;
        }

        public async Task Initialize()
        {
            if (initialized)
                return;

            await semaphore.WaitAsync();
            try
            {
                Bags.Clear();

                foreach (var b in await metaStorage.LoadBags(TrainId))
                    Bags.Add(new Bag(TrainId, b, metaStorage, bucketFactory));

                initialized = true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        bool IsTheSame(string a, string b)
        {
            var dist_diff = System.Math.Abs(a.Length - b.Length);
            var mid_l = (a.Length + b.Length + 1) / 2;
            if ((double)dist_diff / mid_l > (1.0 - LThres))
                return false;

            if (costsTempBuffer.Length < System.Math.Max(a.Length, b.Length))
                costsTempBuffer = new int[System.Math.Max(a.Length, b.Length)];

            var stepsToSame = StringLEV.Distance(a, b, costsTempBuffer);
            var ls2 = (1.0 - (stepsToSame / (double)Math.Max(a.Length, b.Length)));

            return ls2 > LThres;
        }

        public async Task<LogEntry> ReadEntry(EntryAddress address)
        {
            if (address.TrainId != TrainId)
                throw new InvalidOperationException("Wrong train ID");

            if (!initialized)
                await Initialize();

            await semaphore.WaitAsync();
            try
            {
                var bag = Bags[address.BagId];
                var entry = await bag.ReadEntry(address);
                return entry;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<EntryAddress> Push(Base.LogEntry logEntry)
        {
            if (!initialized)
                await Initialize();

            await semaphore.WaitAsync();
            try
            {
                AllCount++;

                int finded = -1;
                for (int oiIndex = 0; oiIndex < Bags.Count; ++oiIndex)
                    if (IsTheSame(Bags[oiIndex].BaseMessage, logEntry.Message))
                    {
                        finded = oiIndex;
                        break;
                    }

                if (finded >= 0)
                {
                    var oib = Bags[finded];
                    var id = await oib.AddMessage(logEntry);

                    int rIndex = finded;
                    while (rIndex > 0 && oib.CurrentBucketId > Bags[rIndex - 1].CurrentBucketId)
                    {
                        Bags.RemoveAt(rIndex);
                        Bags.Insert(rIndex - 1, oib);
                        rIndex--;
                    }
                    return id;
                }

                var newBagInfo = new BagInfo
                {
                    Address = new BagAddress
                    {
                        TrainId = TrainId,
                        BagId = Bags.Count + 1
                    },
                    BaseMessage = logEntry.Message,
                    BagSettings = new BagSettings { PerBucketMessages = 5000 }
                };

                await metaStorage.RegisterNewBag(TrainId, newBagInfo);

                var noib = new Bag(TrainId, newBagInfo, metaStorage, bucketFactory);

                Bags.Add(noib);
                return await noib.AddMessage(logEntry);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Close()
        {
            foreach (var b in Bags)
                await b.Close();

            Bags.Clear();
        }
    }
}
