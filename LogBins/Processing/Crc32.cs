using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LogBins.Processing
{
    public static class Crc32
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;

        private static readonly UInt32[] table = InitializeTable(DefaultPolynomial);

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            var createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                var entry = (UInt32)i;
                for (int j = 0; j < 8; j++) entry = (entry & 1) == 1 ? (entry >> 1) ^ polynomial : entry >> 1;
                createTable[i] = entry;
            }

            return createTable;
        }

        public static UInt32 CalculateHash(ReadOnlySpan<char> buffer, int start, int size)
        {
            var crc = DefaultSeed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    ushort v = (ushort)buffer[i];
                    crc = (crc >> 8) ^ table[(v >> 8) ^ crc & 0xff];
                    crc = (crc >> 8) ^ table[(v & 0xff) ^ crc & 0xff];
                }

            return crc;
        }
    }
}
