using Logs.Server.Core.Server.Base;
using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Tests
{
    [TestFixture]
    class ServerCoreTests
    {
        [Test]
        [TestCase(@"..\..\..\..\TestsData\lgs\syslog_short.zip")]
        public void Store(string file)
        {
            using (var kernel = new StandardKernel(new NinjectSettings { LoadExtensions = false }))
            {
                kernel.Load<Logs.Server.Core.Server.Module>();
                kernel.Load<Logs.Server.Core.Server.ModuleDefault>();
                kernel.Load<Logs.Server.Core.Storage.Module>();

                var server = kernel.Get<IServer>();

                long? lastTicks = null;
                foreach (var m in Tools.ZipLogs.LoadLines(file))
                {
                    var ticks = DateTime.UtcNow.Ticks;
                    if (ticks == lastTicks || ticks < lastTicks)
                        ticks++;

                    server.PutMessage(new LogEntry { DateTime = ticks, Message = m });
                    lastTicks = ticks;
                }
            }
        }
    }
}
