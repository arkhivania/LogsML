using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct EntryAddress
    {
        public ushort TrainId { get; set; }
        public uint BagId { get; set; }
        public ushort Index { get; set; }
    }
}
