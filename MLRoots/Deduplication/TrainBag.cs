using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace MLRoots.Deduplication
{
    class TrainBag
    {
        public long AllCount { get; private set; } = 0;
        const double LThres = 0.85;

        int[] costsTempBuffer = new int[100];

        public List<OneItemBag> OneItemBags { get; } = new List<OneItemBag>();

        bool IsTheSame(string a, string b)
        {
            var dist_diff = System.Math.Abs(a.Length - b.Length);
            var mid_l = (a.Length + b.Length + 1) / 2;
            if ((double)dist_diff / mid_l > (1.0 - LThres))
                return false;

            if (costsTempBuffer.Length < System.Math.Max(a.Length, b.Length))
                costsTempBuffer = new int[System.Math.Max(a.Length, b.Length)];

            var stepsToSame = StringLEV.Distance(a, b, costsTempBuffer);
            //var stepsToSame = Fastenshtein.Levenshtein.Distance(a, b);
            var ls2 = (1.0 - (stepsToSame / (double)Math.Max(a.Length, b.Length)));            

            return ls2 > LThres;
        }

        public TrainBag()
        {
            
        }

        public void Push(string message)
        {
            AllCount++;

            int finded = -1;
            for(int oiIndex = 0; oiIndex < OneItemBags.Count; ++oiIndex)
                if (IsTheSame(OneItemBags[oiIndex].BaseMessage, message))
                {
                    finded = oiIndex;
                    break;
                }

            if(finded >= 0)
            {
                var oib = OneItemBags[finded];
                oib.AddMessage(message);

                int rIndex = finded;
                while (rIndex > 0 && oib.CompleteCount > OneItemBags[rIndex - 1].CompleteCount)
                {
                    OneItemBags.RemoveAt(rIndex);
                    OneItemBags.Insert(rIndex - 1, oib);
                    rIndex--;
                }
                return;
            }

            var noib = new OneItemBag(message, (uint)OneItemBags.Count + 1);
            OneItemBags.Add(noib);
        }
    }
}
