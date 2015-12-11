namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Transports;

    class RequestResponseInvocationForMessagesBehavior : Behavior<IncomingLogicalMessageContext>
    {
        RequestResponseStateLookup requestResponseStateLookup;

        public RequestResponseInvocationForMessagesBehavior(RequestResponseStateLookup requestResponseStateLookup)
        {
            this.requestResponseStateLookup = requestResponseStateLookup;
        }

        public override Task Invoke(IncomingLogicalMessageContext context, Func<Task> next)
        {
            AssignResultIfPossible(context.Extensions.Get<IncomingMessage>(), context);

            return next();
        }

        void AssignResultIfPossible(IncomingMessage incomingMessage, IncomingLogicalMessageContext context)
        {
            var result = context.GetCorrelationIdAndCompletionSource(incomingMessage, requestResponseStateLookup);
            if (!result.HasValue)
            {
                return;
            }

            result.TaskCompletionSource.SetResult(context.Message.Instance);

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