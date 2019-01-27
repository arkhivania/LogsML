using System;
using System.Collections.Generic;
using System.Text;

namespace Logs.Server.Core.Server.Base
{
    public struct LogEntry
    {
        public long DateTime { get; set; } // 100 nanoseconds resolution, can be replaced with greater value
        public string Message { get; set; }
    }
}
