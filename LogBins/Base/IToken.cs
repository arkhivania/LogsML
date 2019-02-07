using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public interface IToken
    {
        bool TheSame(IToken token);
    }
}
