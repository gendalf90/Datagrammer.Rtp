using Datagrammer.Middleware;
using Rtp.Protocol;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Rtp
{
    public sealed class RtpPipeBlock : MiddlewareBlock<Datagram, Datagram>, ISourceBlock<RtpMessage>
    {
        private readonly IPropagatorBlock<RtpMessage, RtpMessage> responseBuffer;

        public RtpPipeBlock() : this(new RtpPipeOptions())
        {
        }

        public RtpPipeBlock(RtpPipeOptions options) : base(options?.MiddlewareOptions)
        {
            responseBuffer = new BufferBlock<RtpMessage>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });
        }

        protected override async Task ProcessAsync(Datagram datagram)
        {
            await NextAsync(datagram);

            if (RtpMessage.TryParse(datagram.Buffer, out var message))
            {
                await responseBuffer.SendAsync(message);
            }
        }

        protected override Task AwaitCompletionAsync()
        {
            return responseBuffer.Completion;
        }

        protected override void OnComplete()
        {
            responseBuffer.Complete();
        }

        protected override void OnFault(Exception exception)
        {
            responseBuffer.Fault(exception);
        }

        public RtpMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<RtpMessage> target, out bool messageConsumed)
        {
            return responseBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<RtpMessage> target, DataflowLinkOptions linkOptions)
        {
            return responseBuffer.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<RtpMessage> target)
        {
            responseBuffer.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<RtpMessage> target)
        {
            return ReserveMessage(messageHeader, target);
        }
    }
}
