using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Processing
{
    class Stat : IToken
    {
        private readonly float threshold;

        public HashSet<UInt32> StatDict { get; set; }

        public Stat(float threshold)
        {
            this.threshold = threshold;
        }

        public static Stat Build(string message, float threshold)
        {

            var cnts = new HashSet<UInt32>();
            foreach (var w in TextTool.Words(message.AsMemory()))
            {
                if (w.Length <= 2)
                    continue;

                cnts.Add(Crc32.CalculateHash(w.Span, 0, w.Length));
            }

            return new Stat(threshold)
            {
                StatDict = cnts
            };
        }

        public bool Compare(Stat s2)
        {
            var m_l = (StatDict.Count + s2.StatDict.Count) / 2;
            var diff = System.Math.Abs(s2.StatDict.Count - StatDict.Count);

            if (m_l == 0)
                return true;

            if (diff * 100 / m_l > 10)
                return false;

            HashSet<UInt32> hashIntersect;
            HashSet<UInt32> checkStat;
            if (StatDict.Count <= s2.StatDict.Count)
            {
                hashIntersect = StatDict;
                checkStat = s2.StatDict;
            }
            else
            {
                hashIntersect = s2.StatDict;
                checkStat = StatDict;
            }

            int intersections = 0;
            foreach (var s in checkStat)
                if (hashIntersect.Contains(s))
                    intersections++;

            if (hashIntersect.Count == 0)
                return false;

            return (intersections * 100.0f / hashIntersect.Count) > threshold;
        }

        public bool TheSame(IToken token)
        {
            var statToken = token as Stat;
            if (statToken == null)
                throw new ArgumentException("Not my token");

            return Compare(statToken);
        }
    }
}
