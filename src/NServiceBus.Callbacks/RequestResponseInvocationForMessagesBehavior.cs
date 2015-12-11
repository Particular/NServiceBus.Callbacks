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
            var correlationId = context.GetCorrelationId();

            if (correlationId == null)
            {
                return;
            }

            string version;
            var checkMessageIntent = true;

            if (incomingMessage.Headers.TryGetValue(Headers.NServiceBusVersion, out version))
            {
                if (version.StartsWith("3."))
                {
                    checkMessageIntent = false;
                }
            }

            if (checkMessageIntent && incomingMessage.GetMesssageIntent() != MessageIntentEnum.Reply)
            {
                return;
            }

            TaskCompletionSourceAdapter tcs;
            if (!requestResponseStateLookup.TryGet(correlationId, out tcs))
            {
                return;
            }

            tcs.SetResult(context.Message.Instance);

            context.MessageHandled = true;
            requestResponseStateLookup.RemoveState(correlationId);
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