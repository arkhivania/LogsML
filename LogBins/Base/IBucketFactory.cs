using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Base
{
    public interface IBucketFactory
    {
        Task<IBucket> CreateBucket(BucketAddress address);
    }
}
