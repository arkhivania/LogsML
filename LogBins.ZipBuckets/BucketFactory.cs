using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    public class BucketFactory : IBucketFactory
    {
        private readonly IBucketStoreFactory bucketStoreFactory;

        public BucketFactory(IBucketStreamProvider bucketStreamProvider)
            : this(new ZipStoreFactory(bucketStreamProvider))
        {

        }

        public BucketFactory(IBucketStoreFactory bucketStoreFactory)
        {
            this.bucketStoreFactory = bucketStoreFactory;
        }

        public async Task<IBucket> CreateBucket(BucketAddress address)
        {
            var res = new Bucket(address, bucketStoreFactory);
            await res.Init();
            return res;
        }
    }
}
