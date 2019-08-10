using Datagrammer.Middleware;

namespace Datagrammer.Rtp
{
    public sealed class RtpPipeOptions
    {
        public int ResponseBufferCapacity { get; set; } = 1;

        public MiddlewareOptions MiddlewareOptions { get; set; } = new MiddlewareOptions();
    }
}
