using Rtp.Protocol;
using System;
using Xunit;

namespace Tests.Unit
{
    public class RtpBuilderTests
    {
        [Fact]
        public void BuildMessage()
        {
            var message = new RtpBuilderStep().WithHeaderExtension()
                                              .WithMarker()
                                              .SetPayloadType(111)
                                              .SetSequenceNumber(539)
                                              .SetTimestamp(459793)
                                              .SetSourceIdentifier(546736809)
                                              .AddSource(10)
                                              .AddSource(234897)
                                              .AddSource(354676542)
                                              .SetPayload(new byte[] { 1, 2, 3, 4 })
                                              .SetPaddingBytesLength(3)
                                              .Build();

            Assert.True(message.Span.SequenceEqual(new byte[] 
            {
                179, // 2, true, true, 3
                239, // true, 111
                2, 27, // 539
                0, 7, 4, 17, // 459793
                32, 150, 138, 169, // 546736809
                0, 0, 0, 10, // 10
                0, 3, 149, 145, // 234897
                21, 35, 239, 62, // 354676542
                1, 2, 3, 4, // payload
                0, 0, 3 // padding 3
            }));
        }
    }
}
