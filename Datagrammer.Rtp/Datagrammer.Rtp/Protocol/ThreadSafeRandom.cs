using System;
using System.Threading;

namespace Rtp.Protocol
{
    internal static class ThreadSafeRandom
    {
        private static int seed = new Random().Next();

        private static ThreadLocal<Random> factory = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next()
        {
            return factory.Value.Next();
        }
    }
}
