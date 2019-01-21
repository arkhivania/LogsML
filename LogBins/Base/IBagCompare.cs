using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    interface IBagCompare
    {
        bool TheSame(Bag bag, string message);
    }
}
