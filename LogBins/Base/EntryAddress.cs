using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct EntryAddress
    {
        public short TrainId { get; set; }
        public int BagId { get; set; }
        public int Index { get; set; }
    }
}
