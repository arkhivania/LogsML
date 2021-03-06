﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogBins.Base;

namespace LogBins.Simple
{
    class Bucket : IBucket
    {
        readonly List<string> entries = new List<string>();
        private readonly IBucketStoreFactory bucketStoreFactory;
        bool isModified = false;
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

        public Task<AddEntryResult> AddEntry(string logEntry)
        {
            isModified = true;
            entries.Add(logEntry);
            var id = AddressTools.MakeAddress(Info.TrainId, Info.BagId, entries.Count - 1);            

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

        public Task Store()
        {
            if (isModified)
            {
                store.StoreEntries(entries);
                isModified = false;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetEntry(int index)
        {
            return Task.FromResult(entries[index]);
        }
    }
}
