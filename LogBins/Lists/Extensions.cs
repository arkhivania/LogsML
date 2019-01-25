using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Lists
{
    public static class Extensions
    {
        public static void Add<TValue>(this SkipList<int, TValue> sl, int key, TValue value)
        {
            sl.AddBD(key, value, (a,b) => System.Math.Abs(a - b));
        }

        public static void Add<TValue>(this SkipList<double, TValue> sl, double key, TValue value)
        {
            sl.AddBD(key, value, (a, b) => (float)System.Math.Abs(a - b));
        }
    }
}
