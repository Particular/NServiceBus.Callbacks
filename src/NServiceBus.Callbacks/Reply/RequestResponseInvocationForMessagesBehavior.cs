namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

    class RequestResponseInvocationForMessagesBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        RequestResponseStateLookup requestResponseStateLookup;

        public RequestResponseInvocationForMessagesBehavior(RequestResponseStateLookup requestResponseStateLookup)
        {
            this.requestResponseStateLookup = requestResponseStateLookup;
        }

        public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            AssignResultIfPossible(context.Extensions.Get<IncomingMessage>(), context);

            return next();
        }

        void AssignResultIfPossible(IncomingMessage incomingMessage, IIncomingLogicalMessageContext context)
        {
            var result = context.GetCorrelationIdAndCompletionSource(incomingMessage, requestResponseStateLookup);
            if (!result.HasValue)
            {
                return;
            }

            result.TaskCompletionSource.TrySetResult(context.Message.Instance);

            context.MessageHandled = true;
            requestResponseStateLookup.RemoveState(result.CorrelationId);
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("RequestResponseInvocationForMessagesBehavior", typeof(RequestResponseInvocationForMessagesBehavior), "Invokes the callback of a synchronous request/response")
            {
            }
        }
    }
}