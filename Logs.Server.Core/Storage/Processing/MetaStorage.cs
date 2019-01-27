using LogBins.Base;
using Logs.Server.Core.Storage.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public Task<BagInfo[]> LoadBags(ushort trainId)
        {
            var fp = Path.Combine(metaFolder, $"bgs_{trainId}");
            if (!File.Exists(fp))
                return Task.FromResult(new BagInfo[] { });

            var c = File.ReadAllText(fp);
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject<BagInfo[]>(c);
            return Task.FromResult(res);
        }

        public async Task RegisterNewBag(ushort trainId, BagInfo bagInfo)
        {
            var bags = new List<BagInfo>(await LoadBags(trainId));
            bags.Add(bagInfo);
            var fp = Path.Combine(metaFolder, $"bgs_{trainId}");
            File.WriteAllText(fp, 
                JsonConvert.SerializeObject(bags.ToArray()));
        }

        public Task StoreCurrentBucketIndexForBag(BagAddress bagAddress, int id)
        {
            var indexFP = Path.Combine(metaFolder, $"ix_{bagAddress.TrainId}_{bagAddress.BagId}");
            File.WriteAllText(indexFP, id.ToString(CultureInfo.InvariantCulture));
            return Task.CompletedTask;
        }

        public Task<int> GetCurrentBucketIndexForBag(BagAddress bagAddress)
        {
            var indexFP = Path.Combine(metaFolder, $"ix_{bagAddress.TrainId}_{bagAddress.BagId}");
            if (!File.Exists(indexFP))
                return Task.FromResult(0);

            var c = File.ReadAllText(indexFP);
            return Task.FromResult(int.Parse(c, CultureInfo.InvariantCulture));
        }
    }
}
