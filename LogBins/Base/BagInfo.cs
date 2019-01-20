using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct BagInfo
    {
        public BagAddress Address { get; set; }
        public string BaseMessage { get; set; }
        public BagSettings BagSettings { get; set; }
    }
}
