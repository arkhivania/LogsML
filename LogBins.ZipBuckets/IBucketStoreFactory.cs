using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.ZipBuckets
{
    public interface IBucketStoreFactory
    {
        Task<IBucketStore> CreateStore(BucketAddress info);
    }
}
