using LogBins.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBins
{
    public sealed class Bag
    {
        struct LocalBucket
        {
            public IBucket Bucket { get; set; }
            public int MessagesCount { get; set; }
        }

        private readonly ushort trainId;
        private readonly IMetaStorage metaStorage;
        private readonly IBucketFactory bucketFactory;
        LocalBucket? currentBucket;

        readonly Dictionary<BucketAddress, IBucket> buckets = new Dictionary<BucketAddress, IBucket>();

        public int CurrentBucketId => currentBucket?.Bucket.Info.BucketId ?? 0;

        public BagInfo BagInfo { get; }
        public string BaseMessage => BagInfo.BaseMessage;

        public Bag(ushort trainId, BagInfo bagInfo,
            IMetaStorage metaStorage,
            IBucketFactory bucketFactory)
        {
            this.trainId = trainId;
            BagInfo = bagInfo;
            this.metaStorage = metaStorage;
            this.bucketFactory = bucketFactory;
            if (bagInfo.BagSettings.PerBucketMessages > (2 << 16))
                throw new ArgumentException("PerBucketMessages must be < 2^16");
        }

        async Task<IBucket> GetBucket(BucketAddress address)
        {
            IBucket bucket;
            if (currentBucket != null && currentBucket.Value.Bucket.Info.Equals(address))
                bucket = currentBucket.Value.Bucket;
            else
            {
                if (!buckets.TryGetValue(address, out bucket))
                    bucket = buckets[address] = await bucketFactory.CreateBucket(address);
            }
            return bucket;
        }

        public async Task<LogEntry> ReadEntry(ulong address)
        {
            var baddr = new BucketAddress
            {
                TrainId = address.TrainId(),
                BagId = address.BagId(),
                BucketId = address.Index() / BagInfo.BagSettings.PerBucketMessages
            };

            var bucket = await GetBucket(baddr);

            return await bucket
                .GetEntry(address.Index() % BagInfo.BagSettings.PerBucketMessages);
        }

        public async Task Init()
        {
            if (currentBucket == null)
            {
                var b_i = await metaStorage
                    .GetCurrentBucketIndexForBag(this.BagInfo.Address);

                var b = await GetBucket(new BucketAddress
                {
                    TrainId = trainId,
                    BagId = BagInfo.Address.BagId,
                    BucketId = b_i
                });

                var count = await b.QueryMessagesCount();
                currentBucket = new LocalBucket
                {
                    Bucket = b,
                    MessagesCount = count
                };
            }
        }

        public async Task<ulong> AddMessage(Base.LogEntry message)
        {
            if (currentBucket == null)
                await Init();            

            if (currentBucket
                .Value
                .MessagesCount == BagInfo.BagSettings.PerBucketMessages)
            {
                var b = await GetBucket(new BucketAddress
                {
                    TrainId = trainId,
                    BagId = BagInfo.Address.BagId,
                    BucketId = currentBucket.Value.Bucket.Info.BucketId + 1
                });

                await metaStorage.StoreCurrentBucketIndexForBag(BagInfo.Address, b.Info.BucketId);

                currentBucket = new LocalBucket
                {
                    Bucket = b,
                    MessagesCount = 0
                };
            }
                
            var entry = await currentBucket.Value
                .Bucket.AddEntry(message);

            currentBucket = new LocalBucket
            {
                Bucket = currentBucket.Value.Bucket,
                MessagesCount = entry.MessagesInBucket
            };

            var index = (currentBucket.Value.Bucket.Info.BucketId * BagInfo.BagSettings.PerBucketMessages) + entry.Index;
            return AddressTools.MakeAddress(BagInfo.Address.TrainId, BagInfo.Address.BagId, index);            
        }

        public async Task Close()
        {
            foreach(var b in buckets.Values)
                await b.Store();
        }        
    }
}
