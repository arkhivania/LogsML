using LogBins.Base;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.SimpleBuckets
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IBucketFactory>()
                .To<LogBins.Simple.BucketFactory>();
        }
    }
}
