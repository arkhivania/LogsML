using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogBins.Base
{
    public interface IBucket
    {
        BucketAddress Info { get; }

        Task<int> QueryMessagesCount();
        Task<AddEntryResult> AddEntry(string logEntry);
        Task<string> GetEntry(int index);

        Task Store();
    }
}
