using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Server.Core.Server.Base
{
    public interface IServer
    {
        Task PutMessage(LogEntry logEntry);
    }
}
