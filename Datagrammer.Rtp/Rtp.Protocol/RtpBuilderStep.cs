using System;

namespace Rtp.Protocol
{
    public readonly struct RtpBuilderStep
    {
        private const int CurrentVersion = 2;
        private const int MinLength = 12;
        private const int IdentifierLength = 4;

        private readonly RtpBuilderState state;

        private RtpBuilderStep(RtpBuilderState state)
        {
            this.state = state;
        }

        public RtpBuilderStep WithHeaderExtension()
        {
            return SetHeaderExtension(true);
        }

        public RtpBuilderStep SetHeaderExtension(bool value)
        {
            var currentState = state;

            currentState.hasHeaderExtension = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep WithMarker()
        {
            return SetMarker(true);
        }

        public RtpBuilderStep SetMarker(bool value)
        {
            var currentState = state;

            currentState.isMarker = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep SetPayloadType(int type)
        {
            if(type < 0 || type > 127)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Must be in range from 0 to 127");
            }

            var currentState = state;

            currentState.payloadType = type;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep SetSequenceNumber(ushort value)
        {
            var currentState = state;

            currentState.sequenceNumber = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep SetTimestamp(int value)
        {
            var currentState = state;

            currentState.timestamp = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep SetSourceIdentifier(int value)
        {
            var currentState = state;

            currentState.sourceIdentifier = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep GenerateSourceIdentifier()
        {
            return SetSourceIdentifier(ThreadSafeRandom.Next());
        }

        public RtpBuilderStep SetPayload(ReadOnlyMemory<byte> bytes)
        {
            var currentState = state;

            currentState.payload = bytes;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep SetPaddingBytesLength(byte value)
        {
            var currentState = state;

            currentState.paddingLength = value;

            return new RtpBuilderStep(currentState);
        }

        public RtpBuilderStep AddSource(int sourceIdentifier)
        {
            var currentState = state;

            AddSourceToState(ref currentState, sourceIdentifier);

            return new RtpBuilderStep(currentState);
        }

        private void AddSourceToState(ref RtpBuilderState state, int sourceIdentifier)
        {
            if (state.sourcesCount == 15)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIdentifier), "Count must be in range from 0 to 15");
            }

            var hasAlreadySet = false;

            TrySetValue(ref state.sourceIdentifier1, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier2, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier3, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier4, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier5, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier6, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier7, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier8, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier9, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier10, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier11, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier12, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier13, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier14, sourceIdentifier, ref hasAlreadySet);
            TrySetValue(ref state.sourceIdentifier15, sourceIdentifier, ref hasAlreadySet);

            state.sourcesCount++;
        }

        private void TrySetValue(ref int? field, int value, ref bool hasAlreadySet)
        {
            if(field.HasValue || hasAlreadySet)
            {
                return;
            }

            field = value;
            hasAlreadySet = true;
        }

        public ReadOnlyMemory<byte> Build()
        {
            var buffer = CreateBuffer();
            var remains = buffer.AsSpan();

            remains = WriteFirst2Bytes(remains);
            remains = WriteSequenceNumber(remains);
            remains = WriteTimestamp(remains);
            remains = WriteSourceIdentifier(remains);
            remains = WriteSources(remains);
            remains = WritePayload(remains);
            WritePadding(remains);

            return buffer;
        }

        private byte[] CreateBuffer()
        {
            var sourcesLength = IdentifierLength * state.sourcesCount;
            return new byte[MinLength + sourcesLength + state.payload.Length + state.paddingLength];
        }

        private Span<byte> WriteFirst2Bytes(Span<byte> bytes)
        {
            bytes[0] = GetFirstByte();
            bytes[1] = GetSecondByte();
            return bytes.Slice(2);
        }

        private byte GetFirstByte()
        {
            var result = CurrentVersion << 6;
            
            if(state.paddingLength > 0)
            {
                result = result | 1 << 5;
            }

            if(state.hasHeaderExtension)
            {
                result = result | 1 << 4;
            }

            result = result | state.sourcesCount & 15;
            return (byte)result;
        }

        private byte GetSecondByte()
        {
            var result = 0;

            if(state.isMarker)
            {
                result = 1 << 7;
            }

            result = result | state.payloadType & 127;
            return (byte)result;
        }

        private Span<byte> WriteSequenceNumber(Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 2), state.sequenceNumber);
            return bytes.Slice(2);
        }

        private Span<byte> WriteTimestamp(Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 4), state.timestamp);
            return bytes.Slice(4);
        }

        private Span<byte> WriteSourceIdentifier(Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 4), state.sourceIdentifier);
            return bytes.Slice(4);
        }

        private Span<byte> WriteSources(Span<byte> bytes)
        {
            var index = 0;

            TryWriteSource(bytes, state.sourceIdentifier1, index++);
            TryWriteSource(bytes, state.sourceIdentifier2, index++);
            TryWriteSource(bytes, state.sourceIdentifier3, index++);
            TryWriteSource(bytes, state.sourceIdentifier4, index++);
            TryWriteSource(bytes, state.sourceIdentifier5, index++);
            TryWriteSource(bytes, state.sourceIdentifier6, index++);
            TryWriteSource(bytes, state.sourceIdentifier7, index++);
            TryWriteSource(bytes, state.sourceIdentifier8, index++);
            TryWriteSource(bytes, state.sourceIdentifier9, index++);
            TryWriteSource(bytes, state.sourceIdentifier10, index++);
            TryWriteSource(bytes, state.sourceIdentifier11, index++);
            TryWriteSource(bytes, state.sourceIdentifier12, index++);
            TryWriteSource(bytes, state.sourceIdentifier13, index++);
            TryWriteSource(bytes, state.sourceIdentifier14, index++);
            TryWriteSource(bytes, state.sourceIdentifier15, index);

            return bytes.Slice(state.sourcesCount * IdentifierLength);
        }

        private void TryWriteSource(Span<byte> bytes, int? sourceIdentifier, int index)
        {
            if(sourceIdentifier.HasValue)
            {
                NetworkBitConverter.WriteBytes(bytes.Slice(index * IdentifierLength, IdentifierLength), sourceIdentifier.Value);
            }
        }

        private Span<byte> WritePayload(Span<byte> bytes)
        {
            state.payload.Span.CopyTo(bytes);
            return bytes.Slice(state.payload.Length);
        }

        private void WritePadding(Span<byte> bytes)
        {
            if(state.paddingLength > 0)
            {
                bytes[bytes.Length - 1] = state.paddingLength;
            }
        }

        private struct RtpBuilderState
        {
            public byte paddingLength;

            public bool hasHeaderExtension;
            public bool isMarker;
            public int payloadType;
            public ushort sequenceNumber;
            public int timestamp;
            public int sourceIdentifier;

            public int sourcesCount;

            public int? sourceIdentifier1;
            public int? sourceIdentifier2;
            public int? sourceIdentifier3;
            public int? sourceIdentifier4;
            public int? sourceIdentifier5;
            public int? sourceIdentifier6;
            public int? sourceIdentifier7;
            public int? sourceIdentifier8;
            public int? sourceIdentifier9;
            public int? sourceIdentifier10;
            public int? sourceIdentifier11;
            public int? sourceIdentifier12;
            public int? sourceIdentifier13;
            public int? sourceIdentifier14;
            public int? sourceIdentifier15;

            public ReadOnlyMemory<byte> payload;
        }
    }
}
