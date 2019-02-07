using LogBins.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogBins.Processing
{
    class Stat : IToken
    {
        public HashSet<string> StatDict { get; set; }

        public static Stat Build(string message)
        {
            var cnts = new HashSet<string>();
            foreach (var w in TextTool.Words(message)
                .Select(q => q.ToLower()))
            {
                if (w.Length <= 2)
                    continue;

                cnts.Add(w);
            }

            return new Stat
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

            HashSet<string> hashIntersect;
            HashSet<string> checkStat;
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

            return intersections * 100 / hashIntersect.Count > 85;
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
