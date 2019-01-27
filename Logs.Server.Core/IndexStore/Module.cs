using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.IndexStore
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<Base.IIndexStore<long, ulong>>()
                .To<Processing.LLStore>();
        }
    }
}
