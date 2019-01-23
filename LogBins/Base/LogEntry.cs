using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public struct LogEntry
    {
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}
