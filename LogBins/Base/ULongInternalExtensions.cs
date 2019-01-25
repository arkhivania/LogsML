using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Base
{
    internal static class ULongInternalExtensions
    {
        public static ushort TrainId(this ulong address)
        {
            return (ushort)(address >> 48);
        }

        public static ushort BagId(this ulong address)
        {
            return (ushort)((address >> 32) & 0xFFFF);
        }

        public static int Index(this ulong address)
        {
            return (int)(address & 0xFFFFFFFF);
        }
    }
}
