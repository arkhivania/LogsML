using LogBins.Simple;
using LogBins.ZipBuckets;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.Storage
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IBucketStreamProvider>()
                .To<Processing.FolderStreamProvider>();

            Kernel.Bind<IBucketStoreFactory>()
                .To<LogBins.ZipBuckets.ZipStoreFactory>();
        }
    }
}
