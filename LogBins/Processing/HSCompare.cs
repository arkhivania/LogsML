using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Processing
{
    public class HSCompare : IBagCompare
    {
        public IToken GetMessageToken(string message)
        {
            return Stat.Build(message);
        }
    }
}
