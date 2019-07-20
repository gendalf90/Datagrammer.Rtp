using System;

namespace Rtp.Protocol
{
    public struct SourceIdentifierEnumerator
    {
        private const int IdentifierLength = 4;

        private int? currentSourceIdentifier;
        private ReadOnlyMemory<byte> remainsOfBytes;

        public SourceIdentifierEnumerator(ReadOnlyMemory<byte> bytes)
        {
            currentSourceIdentifier = null;
            remainsOfBytes = bytes;
        }

        public int Current => currentSourceIdentifier ?? throw new ArgumentOutOfRangeException(nameof(Current));

        public bool MoveNext()
        {
            currentSourceIdentifier = null;

            if(remainsOfBytes.IsEmpty)
            {
                return false;
            }

            var identifierBytes = remainsOfBytes.Span.Slice(0, IdentifierLength);
            currentSourceIdentifier = NetworkBitConverter.ToInt32(identifierBytes);
            remainsOfBytes = remainsOfBytes.Slice(IdentifierLength);
            return true;
        }
    }
}
