using LogBins.Base;
using LogBins.ZipBuckets;
using Logs.Server.Core.Storage.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Logs.Server.Core.Storage.Processing
{
    class FolderStreamProvider : IBucketStreamProvider
    {
        private readonly StorageSettings storageSettings;

        readonly string bucketsFolderPath;

        public FolderStreamProvider(StorageSettings storageSettings)
        {
            this.storageSettings = storageSettings;
            if (!Directory.Exists(storageSettings.TargetFolder))
                throw new InvalidOperationException($"Folder '{storageSettings.TargetFolder}' doesn't exist");

            bucketsFolderPath = Path.Combine(storageSettings.TargetFolder, "Buckets");
            if (!Directory.Exists(bucketsFolderPath))
                Directory.CreateDirectory(bucketsFolderPath);
        }

        string BucketPath(BucketAddress bucketAddress)
        {
            return Path.Combine(bucketsFolderPath,
                $"{bucketAddress.TrainId:D3}/{bucketAddress.BagId:D3}/{bucketAddress.BucketId:D3}");
        }

        public Stream OpenRead(BucketAddress bucketAddress)
        {
            var fileName = BucketPath(bucketAddress);

            if (!File.Exists(fileName))
                return null;

            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public Stream OpenWrite(BucketAddress bucketAddress)
        {
            var fileName = BucketPath(bucketAddress);
            var folder = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return new FileStream(fileName, FileMode.Create);
        }
    }
}
