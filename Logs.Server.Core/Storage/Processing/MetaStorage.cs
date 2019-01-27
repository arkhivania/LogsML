using LogBins.Base;
using Logs.Server.Core.Storage.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Server.Core.Storage.Processing
{
    class MetaStorage : IMetaStorage
    {
        private readonly StorageSettings storageSettings;

        readonly string metaFolder;

        public MetaStorage(StorageSettings storageSettings)
        {
            this.storageSettings = storageSettings;
            if (!Directory.Exists(storageSettings.TargetFolder))
                throw new InvalidOperationException($"Folder '{storageSettings.TargetFolder}' doesn't exist");
            metaFolder = Path.Combine(storageSettings.TargetFolder, "Meta");
            if (!Directory.Exists(metaFolder))
                Directory.CreateDirectory(metaFolder);
        }

        public Task<int> GetCurrentBucketIndexForBag(BagAddress bagAddress)
        {
            throw new NotImplementedException();
        }

        public Task<BagInfo[]> LoadBags(ushort trainId)
        {
            throw new NotImplementedException();
        }

        public Task RegisterNewBag(ushort trainId, BagInfo bagInfo)
        {
            throw new NotImplementedException();
        }

        public Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, int id)
        {
            throw new NotImplementedException();
        }
    }
}
