using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    class Bucket : IBucket, IDisposable
    {
        readonly List<LogEntry> entries = new List<LogEntry>();
        private readonly IBucketStoreFactory bucketStoreFactory;
        bool isModified = false;
        public BucketAddress Info { get; }

        public Task<int> MessagesCount => Task.FromResult(entries.Count);

        IBucketStore store;

        public Bucket(BucketAddress bucketInfo, 
            LogBins.Base.IBucketStoreFactory bucketStoreFactory)
        {
            this.Info = bucketInfo;
            this.bucketStoreFactory = bucketStoreFactory;
        }

        public async Task Init()
        {
            store = await bucketStoreFactory
                .CreateStore(Info);
            entries.AddRange(store.LoadEntries());
        }

        public Task<AddEntryResult> AddEntry(LogEntry logEntry)
        {
            isModified = true;
            entries.Add(logEntry);
            var id = new EntryAddress
            {
                TrainId = Info.TrainId,
                BagId = Info.BagId,
                Index = entries.Count - 1
            };

            var res = new AddEntryResult
            {
                Address = id,
                MessagesInBucket = entries.Count
            };
            return Task.FromResult(res);
        }

        public void Dispose()
        {
            if(isModified)
                store.StoreEntries(entries);
        }

        public Task<int> QueryMessagesCount()
        {
            return Task.FromResult(entries.Count);
        }
    }
}
