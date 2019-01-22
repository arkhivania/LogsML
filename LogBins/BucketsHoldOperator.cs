using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogBins
{
    class BucketsHoldOperator : IDisposable
    {
        class BucketHolder
        {
            public DateTime LastAccess { get; set; }
            public IBucket Bucket { get; set; }
        }

        DateTime badTime;

        readonly Timer badTimeTimer;

        public BucketsHoldOperator(IBucketFactory bucketFactory)
        {   
            badTime = DateTime.UtcNow;
            badTimeTimer = new Timer(t => badTime = DateTime.UtcNow, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
            this.bucketFactory = bucketFactory;
        }        

        readonly Dictionary<BucketAddress, BucketHolder> holders = new Dictionary<BucketAddress, BucketHolder>();
        private readonly IBucketFactory bucketFactory;

        public async Task<IBucket> GetBucket(BucketAddress bucketAddress)
        {
            if (holders.TryGetValue(bucketAddress, out BucketHolder holder))
            {
                holder.LastAccess = badTime;
                await Clean(new[] { holder });
                return holder.Bucket;
            }

            var b = await bucketFactory.CreateBucket(bucketAddress);
            var new_holder = new BucketHolder { Bucket = b, LastAccess = badTime };
            holders[bucketAddress] = new_holder;
            await Clean(new[] { new_holder });
            return b;
        }

        private async Task Clean(IEnumerable<BucketHolder> holdHolders)
        {
            var ct = badTime;
            var old_buckets = holders.Values
                .Except(holdHolders)
                .TakeWhile(q => (ct - q.LastAccess) > TimeSpan.FromMinutes(1))
                .ToArray();

            foreach(var ob in old_buckets)
            {
                holders.Remove(ob.Bucket.Info);
                await ob.Bucket.Close();
            }
        }

        public async Task Close()
        {
            foreach (var h in holders)
                await h.Value.Bucket.Close();

            holders.Clear();
        }

        public void Dispose()
        {
            foreach (var h in holders)
                h.Value.Bucket.Close().Wait();

            badTimeTimer.Dispose();
        }
    }
}
