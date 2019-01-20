using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LogBins.Base;

namespace LogBins.ZipBuckets
{
    public class ZipStoreFactory : LogBins.Base.IBucketStoreFactory
    {
        public Task<IBucketStore> CreateStore(BucketAddress info)
        {
            return Task.FromResult<IBucketStore>(new ZipStore(info));
        }
    }
}
