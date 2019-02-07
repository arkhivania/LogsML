using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Processing
{
    public class HSCompare : IBagCompare
    {
        public float PercentsThreshold { get; set; } = 85.0f;

        public IToken GetMessageToken(string message)
        {
            return Stat.Build(message, PercentsThreshold);
        }
    }
}
