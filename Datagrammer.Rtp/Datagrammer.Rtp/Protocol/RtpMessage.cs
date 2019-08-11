using System;

namespace Rtp.Protocol
{
    public readonly struct RtpMessage
    {
        private const int CurrentVersion = 2;
        private const int MinLength = 12;
        private const int IdentifierLength = 4;

        private RtpMessage(int version,
                           bool hasHeaderExtension,
                           bool isMarker,
                           int payloadType,
                           ushort sequenceNumber,
                           int timestamp,
                           int sourceIdentifier,
                           SourceIdentifiers sources,
                           ReadOnlyMemory<byte> payload)
        {
            Version = version;
            HasHeaderExtension = hasHeaderExtension;
            IsMarker = isMarker;
            PayloadType = payloadType;
            SequenceNumber = sequenceNumber;
            Timestamp = timestamp;
            SourceIdentifier = sourceIdentifier;
            Sources = sources;
            Payload = payload;
        }

        public int Version { get; }

        public bool HasHeaderExtension { get; }

        public bool IsMarker { get; }

        public int PayloadType { get; }

        public ushort SequenceNumber { get; }

        public int Timestamp { get; }

        public int SourceIdentifier { get; }

        public SourceIdentifiers Sources { get; }

        public ReadOnlyMemory<byte> Payload { get; }

        public static bool TryParse(ReadOnlyMemory<byte> bytes, out RtpMessage message)
        {
            message = new RtpMessage();

            if(bytes.Length < MinLength)
            {
                return false;
            }

            var first2Bytes = bytes.Span.Slice(0, 2);
            var version = first2Bytes[0] >> 6;

            if (version != CurrentVersion)
            {
                return false;
            }

            var isPadded = Convert.ToBoolean((first2Bytes[0] >> 5) & 1);
            var paddedByteCount = 0;

            if(isPadded)
            {
                paddedByteCount = bytes.Span[bytes.Length - 1];
            }

            if(paddedByteCount > bytes.Length - MinLength)
            {
                return false;
            }

            var notPaddedBytes = bytes.Slice(0, bytes.Length - paddedByteCount);
            var remainsOfBytes = notPaddedBytes.Slice(MinLength);
            var hasHeaderExtension = Convert.ToBoolean((first2Bytes[0] >> 4) & 1);
            var CSRCCount = first2Bytes[0] & 15;
            var CSRCLength = CSRCCount * IdentifierLength;
            
            if(remainsOfBytes.Length < CSRCLength)
            {
                return false;
            }

            var CSRCBytes = remainsOfBytes.Slice(0, CSRCLength);
            var payload = remainsOfBytes.Slice(CSRCLength);
            var sources = new SourceIdentifiers(CSRCBytes, CSRCCount);
            var isMarker = Convert.ToBoolean(first2Bytes[1] >> 7);
            var payloadType = first2Bytes[1] & 127;
            var sequenceNumber = NetworkBitConverter.ToUInt16(bytes.Span.Slice(2, 2));
            var timestamp = NetworkBitConverter.ToInt32(bytes.Span.Slice(4, 4));
            var sourceIdentifier = NetworkBitConverter.ToInt32(bytes.Span.Slice(8, 4));

            message = new RtpMessage(version,
                                     hasHeaderExtension,
                                     isMarker,
                                     payloadType,
                                     sequenceNumber,
                                     timestamp,
                                     sourceIdentifier,
                                     sources,
                                     payload);
            return true;
        }
    }
}
