using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLRoots.StringMath
{
    public static class Levenstein
    {
        public static double Distance(string s1, string s2)
        {
            if (s1 == null)
                throw new ArgumentNullException(nameof(s1));

            if (s2 == null)
                throw new ArgumentNullException(nameof(s2));

            if (s1 == s2)
                return 0;

            if (s1.Length == 0)
                return s2.Length;

            if (s2.Length == 0)
                return s1.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[s2.Length + 1];
            int[] v1 = new int[s2.Length + 1];
            int[] vtemp;
            
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < s1.Length; i++)
            {
                v1[0] = i + 1;

                for (int j = 0; j < s2.Length; j++)
                {
                    int cost = 1;
                    if (s1[i] == s2[j])
                        cost = 0;

                    v1[j + 1] = System.Math.Min(
                            v1[j] + 1,              // Cost of insertion
                            System.Math.Min(
                                    v0[j + 1] + 1,  // Cost of remove
                                    v0[j] + cost)); // Cost of substitution
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                //System.arraycopy(v1, 0, v0, 0, v0.length);
                // Flip references to current and previous row

                vtemp = v0;
                v0 = v1;
                v1 = vtemp;
            }

            return v0[s2.Length];
        }
    }
}
