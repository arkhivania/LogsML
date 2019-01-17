using System;
using System.Collections.Generic;
using System.Text;

namespace MLRoots.Deduplication
{
    class StringLEV
    {
        public static int Distance(string value1, string value2)
        {
            if (value2.Length == 0)
                return value1.Length;

            int[] costs = new int[value2.Length];

            // Add indexing for insertion to first row
            for (int i = 0; i < costs.Length;)
                costs[i] = ++i;

            for (int i = 0; i < value1.Length; i++)
            {
                int cost = i;
                int addationCost = i;

                // cache value for inner loop to avoid index lookup and bonds checking, profiled this is quicker
                char value1Char = value1[i];

                for (int j = 0; j < value2.Length; j++)
                {
                    int insertionCost = cost;
                    cost = addationCost;

                    // assigning this here reduces the array reads we do, improvment of the old version
                    addationCost = costs[j];

                    if (value1Char != value2[j])
                    {
                        if (insertionCost < cost)
                            cost = insertionCost;

                        if (addationCost < cost)
                            cost = addationCost;

                        ++cost;
                    }

                    costs[j] = cost;
                }
            }

            return costs[costs.Length - 1];
        }

    }
}
