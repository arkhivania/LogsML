using System;
using System.Collections.Generic;
using System.Text;

namespace LogBins.Processing
{
    static class TextTool
    {
        public static IEnumerable<string> Words(string text)
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

            if (curLength != 0)
                yield return new string(curBuffer, 0, curLength);
        }
    }
}
