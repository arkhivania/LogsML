﻿using LogBins.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBins
{
    public class Bag
    {
        struct LocalBucket
        {
            public IBucket Bucket { get; set; }
            public int MessagesCount { get; set; }
        }

        private readonly ushort trainId;
        private readonly IMetaStorage metaStorage;
        readonly BucketsHoldOperator bucketsHoldOperator;

        LocalBucket? currentBucket;

        public uint CurrentBucketId => currentBucket?.Bucket.Info.BucketId ?? 0;

        public BagInfo BagInfo { get; }
        public string BaseMessage => BagInfo.BaseMessage;

        public Bag(ushort trainId, BagInfo bagInfo,
            IMetaStorage metaStorage,
            IBucketFactory bucketFactory)
        {
            this.trainId = trainId;
            BagInfo = bagInfo;
            this.metaStorage = metaStorage;

            if (bagInfo.BagSettings.PerBucketMessages > (2 << 16))
                throw new ArgumentException("PerBucketMessages must be < 2^16");

            this.bucketsHoldOperator = new BucketsHoldOperator(bucketFactory);

            
        }

        public async Task<LogEntry> ReadEntry(EntryAddress address)
        {
            var b = await bucketsHoldOperator.GetBucket(new BucketAddress
            {
                TrainId = address.TrainId,
                BagId = address.BagId,
                BucketId = (uint)(address.Index / BagInfo.BagSettings.PerBucketMessages)
            });

            return await b.GetEntry(address.Index);
        }

        public async Task Init()
        {
            if (currentBucket == null)
            {
                var b_i = await metaStorage.GetCurrentBucketIndexForBag(this.BagInfo.Address);
                var b = await bucketsHoldOperator.GetBucket(new BucketAddress
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

        public async Task<EntryAddress> AddMessage(Base.LogEntry message)
        {
            if (currentBucket == null)
                await Init();            

            if (currentBucket
                .Value
                .MessagesCount == BagInfo.BagSettings.PerBucketMessages)
            {
                var b = await bucketsHoldOperator.GetBucket(new BucketAddress
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

        public async Task Close()
        {
            await bucketsHoldOperator.Close();
        }
    }
}
