using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.Server
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<Base.IServer>()
                .To<Processing.Server>()
                .InSingletonScope();
        }
    }
}
