﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    class Bucket : IBucket
    {
        readonly List<LogEntry> entries = new List<LogEntry>();
        private readonly IBucketStoreFactory bucketStoreFactory;
        bool isModified = false;
        bool isClosed = false;
        public BucketAddress Info { get; }

        public Task<int> MessagesCount => Task.FromResult(entries.Count);

        IBucketStore store;

        public Bucket(BucketAddress bucketInfo, 
            IBucketStoreFactory bucketStoreFactory)
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
                Index = (ushort)(entries.Count - 1)
            };

            var res = new AddEntryResult
            {
                Index = entries.Count - 1,
                MessagesInBucket = entries.Count
            };            
            return Task.FromResult(res);
        }

        public Task<int> QueryMessagesCount()
        {
            return Task.FromResult(entries.Count);
        }

        public Task Close()
        {
            if (isModified)
            {
                store.StoreEntries(entries);
                isModified = false;
            }

            isClosed = true;
            return Task.CompletedTask;
        }

        public Task<LogEntry> GetEntry(int index)
        {
            return Task.FromResult(entries[index]);
        }
    }
}
