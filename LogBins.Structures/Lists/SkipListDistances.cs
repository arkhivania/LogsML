using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Structures.Lists
{
    public static class SkipListDistances
    {
        public static Func<int, int, float> Int32 
            => new Func<int, int, float>((a,b) => Math.Abs(a - b));
    }
}
