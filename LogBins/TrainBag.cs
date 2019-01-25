using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using LogBins.Base;
using LogBins.Processing;

namespace LogBins
{
    public sealed class TrainBag : IDisposable
    {
        const int MAX_BAGS_INDEX = 65535;

        public long AllCount { get; private set; } = 0;
        
        private readonly IBucketFactory bucketFactory;
        private readonly IMetaStorage metaStorage;
        private readonly BagSettings bagSettings;
        readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        
        public ushort TrainId { get; }
        
        readonly IBagCompare<Stat> compare = new HSCompare();

        public List<Bag> Bags { get; } = new List<Bag>();
        readonly Dictionary<int, Bag> bagIdToBag = new Dictionary<int, Bag>();

        bool initialized = false;

        public TrainBag(ushort trainId,
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

                foreach (var b in Bags)
                    await b.Init();

                Bags.Sort((a,b) => -a.CurrentBucketId.CompareTo(b.CurrentBucketId));
                foreach (var b in Bags)
                    bagIdToBag[b.BagInfo.Address.BagId] = b;

                initialized = true;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<LogEntry> ReadEntry(ulong address)
        {
            if (address.TrainId() != TrainId)
                throw new InvalidOperationException("Wrong train ID");

            if (!initialized)
                await Initialize();

            await semaphore.WaitAsync();
            try
            {
                var bag = bagIdToBag[address.BagId()];
                var entry = await bag.ReadEntry(address);
                return entry;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<ulong> Push(Base.LogEntry logEntry)
        {
            if (!initialized)
                await Initialize();

            await semaphore.WaitAsync();
            try
            {
                AllCount++;

                int finded = -1;
                var ms = compare.GetMessageToken(logEntry.Message);

                for (int oiIndex = 0; oiIndex < Bags.Count; ++oiIndex)
                    if (compare.TheSame(Bags[oiIndex], ms))
                    {
                        finded = oiIndex;
                        break;
                    }

                if (finded == -1 && Bags.Count == MAX_BAGS_INDEX + 1)
                    finded = Bags.Count - 1;

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
                        BagId = (ushort)Bags.Count
                    },
                    BaseMessage = logEntry.Message,
                    BagSettings = new BagSettings { PerBucketMessages = 5000 }
                };

                await metaStorage.RegisterNewBag(TrainId, newBagInfo);

                var noib = new Bag(TrainId, newBagInfo, metaStorage, bucketFactory);
                await noib.Init();

                bagIdToBag[noib.BagInfo.Address.BagId] = noib;

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
            await semaphore.WaitAsync();

            try
            {
                foreach (var b in Bags)
                    await b.Close();

                Bags.Clear();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            Close().Wait();
        }
    }
}
