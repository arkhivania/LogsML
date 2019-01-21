using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Base
{
    public interface IMetaStorage
    {
        Task<BagInfo[]> LoadBags(ushort trainId);
        Task RegisterNewBag(ushort trainId, BagInfo bagInfo);
        Task<uint> GetCurrentBucketIndexForBag(BagAddress bagAddress);
        Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, uint id);
    }
}
