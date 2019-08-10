using Xunit;
using Rtp.Protocol;
using System;

namespace Tests.Unit
{
    public class RtpMessageTests
    {
        [Fact]
        public void ParseMessage()
        {
            var bytes = new byte[] 
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
            };

            var result = RtpMessage.TryParse(bytes, out var message);
            var sources = message.Sources.GetEnumerator();

            Assert.True(result);
            Assert.Equal(2, message.Version);
            Assert.True(message.HasHeaderExtension);
            Assert.True(message.IsMarker);
            Assert.Equal(111, message.PayloadType);
            Assert.Equal(539, message.SequenceNumber);
            Assert.Equal(459793, message.Timestamp);
            Assert.Equal(546736809, message.SourceIdentifier);

            Assert.Equal(3, message.Sources.Count);
            Assert.True(sources.MoveNext());
            Assert.Equal(10, sources.Current);
            Assert.True(sources.MoveNext());
            Assert.Equal(234897, sources.Current);
            Assert.True(sources.MoveNext());
            Assert.Equal(354676542, sources.Current);

            Assert.True(message.Payload.Span.SequenceEqual(new byte[] { 1, 2, 3, 4 }));
        }
    }
}
