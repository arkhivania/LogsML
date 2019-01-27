﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogBins.Base;
using Logs.Server.Core.Server.Base;

namespace Logs.Server.Core.Server.Processing
{
    class Server : IServer
    {
        readonly LogBins.TrainBag[] trainBags;

        readonly SemaphoreSlim[] trainBagsSemaphores;
        readonly Random random = new Random();

        public Server(Settings settings, IMetaStorage metaStorage, IBucketFactory bucketFactory)
        {
            trainBags = new LogBins.TrainBag[settings.TrainsCount];
            trainBagsSemaphores = new SemaphoreSlim[settings.TrainsCount];

            for (int i = 0; i < trainBags.Length; ++i)
            {
                trainBags[i] = new LogBins.TrainBag((ushort)(i + 1),
                    bucketFactory, metaStorage,
                    new LogBins.BagSettings
                    {
                        PerBucketMessages = 8192
                    });
                trainBagsSemaphores[i] = new SemaphoreSlim(1, 1);
            }
        }

        public async Task PutMessage(LogEntry logEntry)
        {
            var index = random.Next(trainBags.Length);
            await trainBagsSemaphores[index].WaitAsync();

            try
            {
                await trainBags[index].Push(logEntry.Message);
            }finally
            {
                trainBagsSemaphores[index].Release();
            }
        }
    }
}