using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct BucketAddress
    {
        public ushort TrainId { get; set; }
        public uint BagId { get; set; }
        public uint BucketId { get; set; }
    }
}
