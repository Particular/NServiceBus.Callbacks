﻿namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Transports;

    class RequestResponseInvocationForMessagesBehavior : Behavior<IIncomingLogicalMessageContext>
    {
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
            var result = context.TryRemoveResponseStateBasedOnCorrelationId(incomingMessage, requestResponseStateLookup);
            if (!result.HasValue)
            {
                return;
            }

            result.Value.TaskCompletionSource.TrySetResult(context.Message.Instance);
            context.MessageHandled = true;
        }

        RequestResponseStateLookup requestResponseStateLookup;

        public class Registration : RegisterStep
        {
            public Registration()
                : base("RequestResponseInvocationForMessagesBehavior", typeof(RequestResponseInvocationForMessagesBehavior), "Invokes the callback of a synchronous request/response")
            {
            }
        }
    }
}