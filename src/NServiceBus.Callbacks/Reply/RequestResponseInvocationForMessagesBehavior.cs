namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Transport;

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

            try
            {
                result.Value.TaskCompletionSource.TrySetResult(context.Message.Instance);
                context.MessageHandled = true;
            }
            catch (Exception e)
            {
                result.Value.TaskCompletionSource.TrySetException(e);
                throw;
            }
        }

        RequestResponseStateLookup requestResponseStateLookup;
    }
}