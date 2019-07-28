using System;

namespace Rtp.Protocol
{
    public readonly struct SourceIdentifiers
    {
        private readonly ReadOnlyMemory<byte> bytes;

        internal SourceIdentifiers(ReadOnlyMemory<byte> bytes, int count)
        {
            this.bytes = bytes;

            Count = count;
        }

        public int Count { get; }

        public SourceIdentifierEnumerator GetEnumerator()
        {
            return new SourceIdentifierEnumerator(bytes);
        }
    }
}
