using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Processing
{
    class HSCompare : Base.IBagCompare
    {
        class Stat
        {
            public HashSet<string> StatDict { get; set; }

            

            public bool Compare(Stat s2)
            {
                var m_l = (StatDict.Count + s2.StatDict.Count) / 2;
                var diff = System.Math.Abs(s2.StatDict.Count - StatDict.Count);
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

                return intersections * 100 / hashIntersect.Count > 85;
            }
        }

        readonly Dictionary<Bag, Stat> stat = new Dictionary<Bag, Stat>();

        static IEnumerable<string> Words(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            char[] curBuffer = new char[24];
            int curLength = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (char.IsLetter(c))
                {
                    if (curBuffer.Length <= curLength)
                        Array.Resize(ref curBuffer, curBuffer.Length * 2);

                    curBuffer[curLength] = c;
                    curLength++;
                }
                else if (curLength != 0)
                {
                    yield return new string(curBuffer, 0, curLength);
                    curLength = 0;
                }
            }

            if(curLength != 0)
                yield return new string(curBuffer, 0, curLength);
        }

        Stat BuildStat(string message)
        {
            var cnts = new HashSet<string>();
            foreach (var w in Words(message)
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

        public bool TheSame(Bag bag, string message)
        {
            if (stat.TryGetValue(bag, out Stat s))
                return s.Compare(BuildStat(message));

            var s2 = stat[bag] = BuildStat(bag.BaseMessage);
            return s2.Compare(BuildStat(message));
        }
    }
}
