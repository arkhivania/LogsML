using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    interface IBagCompare<IToken>
    {
        IToken GetMessageToken(string message);
        bool TheSame(Bag bag, IToken token);
    }
}
