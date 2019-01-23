using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Index
{
    public struct IndexEntry
    {
        public DateTime EntryDateTime { get; set; }
        public EntryAddress Address { get; set; }
    }
}
