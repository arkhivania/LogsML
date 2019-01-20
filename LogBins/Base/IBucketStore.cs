using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Base
{
    public interface IBucketStore
    {
        IEnumerable<LogEntry> LoadEntries();
        void StoreEntries(IEnumerable<LogEntry> entries);
    }
}
