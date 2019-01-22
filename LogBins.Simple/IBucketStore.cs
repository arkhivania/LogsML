using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Simple
{
    public interface IBucketStore
    {
        IEnumerable<LogEntry> LoadEntries();
        void StoreEntries(IEnumerable<LogEntry> entries);
    }
}
