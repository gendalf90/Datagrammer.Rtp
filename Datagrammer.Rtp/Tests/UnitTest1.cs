using System;
using System.Collections.Specialized;
using Xunit;
using Rtp.Protocol;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var v = new BitVector32(13);

            var b1 = v.GetBit(0);
            var b2 = v.GetBit(1);
            var b3 = v.GetBit(2);
            var b4 = v.GetBit(3);

            var s1 = v.GetInt32(1, 2);

            var i1 = (13 >> 2) & 1;
        }
    }
}
