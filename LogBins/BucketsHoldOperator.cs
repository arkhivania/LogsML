using LogBins.Base;
using Maybe.SkipList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogBins
{
    class BucketsHoldOperator
    {
        class BucketHolder
        {
            public DateTime LastAccess { get; set; }
            public IBucket Bucket { get; set; }
        }

        class TimeComparer : IComparer<BucketHolder>
        {
            public int Compare(BucketHolder x, BucketHolder y)
            {
                return x.LastAccess.CompareTo(y.LastAccess);
            }
        }

        DateTime badTime;

        readonly Timer badTimeTimer;

        readonly SkipList<BucketHolder> skipHolders = new SkipList<BucketHolder>(new TimeComparer());

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
                skipHolders.Remove(holder);
                holder.LastAccess = badTime;
                skipHolders.Add(holder);
                await Clean();
                return holder.Bucket;
            }

            var b = await bucketFactory.CreateBucket(bucketAddress);
            var new_holder = new BucketHolder { Bucket = b, LastAccess = badTime };
            holders[bucketAddress] = new_holder;
            skipHolders.Add(new_holder);
            await Clean();
            return b;
        }

        private async Task Clean()
        {
            var ct = badTime;
            var old_buckets = skipHolders
                .TakeWhile(q => (ct - q.LastAccess) > TimeSpan.FromMinutes(1))
                .ToArray();

            foreach(var ob in old_buckets)
            {
                holders.Remove(ob.Bucket.Info);
                skipHolders.Remove(ob);
                await ob.Bucket.Close();
            }
        }

        public async Task Close()
        {
            foreach (var h in holders)
                await h.Value.Bucket.Close();

            holders.Clear();
            badTimeTimer.Dispose();
        }

        
    }
}
