using LogBins.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogBins.Tests.Tools
{
    class MS : IMetaStorage
    {
        readonly List<BagInfo> bagInfos = new List<BagInfo>();
        readonly Dictionary<BagAddress, int> bagBuckets = new Dictionary<BagAddress, int>();

        public Task<int> GetCurrentBucketIndexForBag(BagAddress bagAddress)
        {
            if (bagBuckets.TryGetValue(bagAddress, out int bv))
                return Task.FromResult(bv);

            return Task.FromResult(0);
        }

        public Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, int id)
        {
            bagBuckets[bagAddress] = id;
            return Task.CompletedTask;
        }

        public Task<BagInfo[]> LoadBags(ushort trainId)
        {
            return Task.FromResult(bagInfos.ToArray());
        }

        public Task RegisterNewBag(ushort trainId, BagInfo bagInfo)
        {
            bagInfos.Add(bagInfo);
            return Task.CompletedTask;
        }
    }
}
