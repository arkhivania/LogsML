using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Base
{
    public interface IMetaStorage
    {
        Task<BagInfo[]> LoadBags(short trainId);
        Task RegisterNewBag(short trainId, BagInfo bagInfo);
        Task<int> GetCurrentBucketIndexForBag(BagAddress bagAddress);
        Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, int id);
    }
}
