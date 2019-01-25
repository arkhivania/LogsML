using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Index
{
    public struct IndexEntry
    {
        public ulong Address { get; set; }
        public DateTime EntryDateTime { get; set; }        
    }
}
