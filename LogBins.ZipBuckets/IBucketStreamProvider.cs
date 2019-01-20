using LogBins.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogBins.ZipBuckets
{
    public interface IBucketStreamProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucketAddress"></param>
        /// <returns>null if there is no stored bucket</returns>
        Stream OpenRead(BucketAddress bucketAddress);
        Stream OpenWrite(BucketAddress bucketAddress);
    }
}
