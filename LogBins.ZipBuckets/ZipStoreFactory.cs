using System.Threading.Tasks;
using LogBins.Base;
using LogBins.Simple;

namespace LogBins.ZipBuckets
{
    public class ZipStoreFactory : IBucketStoreFactory
    {
        private readonly IBucketStreamProvider bucketStreamProvider;

        public ZipStoreFactory(IBucketStreamProvider bucketStreamProvider)
        {
            this.bucketStreamProvider = bucketStreamProvider;
        }

        public Task<IBucketStore> CreateStore(BucketAddress info)
        {
            return Task.FromResult<IBucketStore>(new ZipStore(info, bucketStreamProvider));
        }
    }
}
