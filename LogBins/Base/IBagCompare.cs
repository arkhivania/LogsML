﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public interface IBagCompare
    {
        IToken GetMessageToken(string message);
    }
}
