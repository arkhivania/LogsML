using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogBins.Processing
{
    class HSCompare : Base.IBagCompare<Stat>
    {
        readonly Dictionary<Bag, Stat> stat = new Dictionary<Bag, Stat>();

        public bool TheSame(Bag bag, Stat messageStat)
        {
            if (stat.TryGetValue(bag, out Stat s))
                return s.Compare(messageStat);

            var s2 = stat[bag] = Stat.Build(bag.BaseMessage);
            return s2.Compare(messageStat);
        }

        public Stat GetMessageToken(string message)
        {
            return Stat.Build(message);
        }
    }
}
