using System;
using System.Collections.Specialized;

namespace Rtp.Protocol
{
    public static class BitVectorExtensions
    {
        public static bool GetBit(this BitVector32 bits, int bitIndex)
        {
            return Convert.ToBoolean(GetInt32(bits, bitIndex, 1));
        }

        public static void SetBit(this BitVector32 bits, int bitIndex, bool value)
        {
            SetInt32(bits, bitIndex, 1, Convert.ToInt32(value));
        }

        public static int GetInt32(this BitVector32 bits, int offset, int length)
        {
            return bits[CreateSection(offset, length)];
        }

        public static void SetInt32(this BitVector32 bits, int offset, int length, int value)
        {
            bits[CreateSection(offset, length)] = value;
        }

        private static BitVector32.Section CreateSection(int offset, int length)
        {
            var maxValue = Convert.ToInt16(GetMaxValueByIndex(length) - 1);
            var offsetSection = CreateOffsetSection(offset);
            return BitVector32.CreateSection(maxValue, offsetSection);
        }

        private static BitVector32.Section CreateOffsetSection(int offset)
        {
            if(offset == 0)
            {
                return new BitVector32.Section();
            }

            var offsetValue = Convert.ToInt16(GetMaxValueByIndex(offset) - 1);
            return BitVector32.CreateSection(offsetValue);
        }

        private static int GetMaxValueByIndex(int bitIndex)
        {
            return Convert.ToInt32(Math.Pow(2, bitIndex));
        }
    }
}
