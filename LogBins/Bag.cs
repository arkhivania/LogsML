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
    class Bag
    {
        struct LocalBucket
        {
            public IBucket Bucket { get; set; }
            public int MessagesCount { get; set; }
        }

        private readonly short trainId;
        private readonly IMetaStorage metaStorage;
        private readonly IBucketFactory bucketFactory;

        LocalBucket? currentBucket;

        public int CurrentBucketId { get; }

        public BagInfo BagInfo { get; }
        public string BaseMessage => BagInfo.BaseMessage;

        public Bag(short trainId, BagInfo bagInfo,
            IMetaStorage metaStorage,
            IBucketFactory bucketFactory)
        {
            this.trainId = trainId;
            BagInfo = bagInfo;
            this.metaStorage = metaStorage;
            this.bucketFactory = bucketFactory;
        }

        public async Task<EntryAddress> AddMessage(Base.LogEntry message)
        {
            if(currentBucket == null)
            {
                var b_i = await metaStorage.GetCurrentBucketIndexForBag(this.BagInfo.Address);
                var b = await bucketFactory.CreateBucket(new BucketAddress
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

            if (currentBucket
                .Value
                .MessagesCount == BagInfo.BagSettings.PerBucketMessages)
            {
                var b = await bucketFactory.CreateBucket(new BucketAddress
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
            return entry.Address;
        }
    }
}
