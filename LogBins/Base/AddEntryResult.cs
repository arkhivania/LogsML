using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct AddEntryResult
    {
        public int MessagesInBucket { get; set; }
        public EntryAddress Address { get; set; }
    }
}
