using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    public static class AddressTools
    {
        public static ulong MakeAddress(ushort trainId, ushort bagID, int index)
        {
            if (index < 0)
                throw new InvalidOperationException("index can't be < 0");

            return (ulong)trainId << 48 | (ulong)bagID << 32 | (ulong)(uint)index;
        }
    }
}
