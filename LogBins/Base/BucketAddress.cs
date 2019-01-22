using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct BucketAddress
    {
        public ushort TrainId { get; set; }
        public int BagId { get; set; }
        public int BucketId { get; set; }
    }
}
