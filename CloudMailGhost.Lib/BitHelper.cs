using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudMailGhost.Lib
{
    public static class BitHelper
    {
        public static void SetBitInByte(ref byte targetByte, int bitIndex, bool value)
        {
            if (bitIndex < 0 || bitIndex > 7) throw new ArgumentOutOfRangeException("" + bitIndex);

            if (value)
                targetByte |= (byte)(1 << bitIndex);
            else
                targetByte &= (byte)~(1 << bitIndex);
        }

        public static bool GetBitFromByte(byte sourceByte, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7) throw new ArgumentOutOfRangeException("" + bitIndex);

            return (sourceByte & (1 << bitIndex)) != 0;
        }
    }
}
