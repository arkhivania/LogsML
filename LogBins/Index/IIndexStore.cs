using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Index
{
    public interface IIndexStore
    {
        void AddEntries(IEnumerable<IndexEntry> entries);
    }
}
