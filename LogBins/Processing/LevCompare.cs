using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Processing
{
    class LevCompare : Base.IBagCompare
    {
        const double LThres = 0.85;
        int[] costsTempBuffer = new int[100];

        bool IsTheSame(string a, string b)
        {
            var dist_diff = System.Math.Abs(a.Length - b.Length);
            var mid_l = (a.Length + b.Length + 1) / 2;
            if ((double)dist_diff / mid_l > (1.0 - LThres))
                return false;

            if (costsTempBuffer.Length < System.Math.Max(a.Length, b.Length))
                costsTempBuffer = new int[System.Math.Max(a.Length, b.Length)];

            var stepsToSame = StringLEV.Distance(a, b, costsTempBuffer);
            var ls2 = (1.0 - (stepsToSame / (double)Math.Max(a.Length, b.Length)));

            return ls2 > LThres;
        }

        public bool TheSame(Bag bag, string message)
        {
            return IsTheSame(bag.BaseMessage, message);
        }
    }
}
