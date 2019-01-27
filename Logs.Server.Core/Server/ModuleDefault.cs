using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.Server
{
    public class ModuleDefault : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<Base.Settings>()
                .ToConstant(new Base.Settings
                {
                    TrainsCount = 4
                });
        }
    }
}
