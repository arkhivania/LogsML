using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Processing
{
    static class TextTool
    {
        public static IEnumerable<ReadOnlyMemory<char>> Words(ReadOnlyMemory<char> text)
        {
            if (text.Length == 0)
                yield break;

            int curStart = 0;
            int curLength = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text.Span[i];
                if (char.IsLetter(c))
                    curLength++;
                else
                {
                    if (curLength != 0)
                    {
                        yield return text.Slice(curStart, curLength);
                        curLength = 0;                        
                    }

                    curStart = i;
                }
            }

            if (curLength != 0)
                yield return text.Slice(curStart, curLength);
        }
    }
}
