namespace NServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

    class UpdateRequestResponseCorrelationTableBehavior : Behavior<IOutgoingPhysicalMessageContext>
    {
        RequestResponseStateLookup lookup;

        public UpdateRequestResponseCorrelationTableBehavior(RequestResponseStateLookup lookup)
        {
            this.lookup = lookup;
        }

        public override Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
        {
            RequestResponseParameters parameters;

            if (context.Extensions.TryGet(out parameters) && !parameters.CancellationToken.IsCancellationRequested)
            {
                var messageId = context.MessageId;
                parameters.CancellationToken.Register(() =>
                {
                    TaskCompletionSourceAdapter tcs;
                    if (lookup.TryGet(messageId, out tcs))
                    {
                        tcs.SetCancelled();
                    }
                });
                lookup.RegisterState(messageId, parameters.TaskCompletionSource);
            }

            return next();
        }

        public class RequestResponseParameters
        {
            public RequestResponseParameters()
            {
                CancellationToken = CancellationToken.None;
            }

            public TaskCompletionSourceAdapter TaskCompletionSource;

            public CancellationToken CancellationToken;
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("UpdateRequestResponseCorrelationTable", typeof(UpdateRequestResponseCorrelationTableBehavior), "Updates the correlation table that keeps track of synchronous request/response callbacks")
            {
                InsertAfterIfExists(WellKnownStep.MutateOutgoingTransportMessage);
            }
        }
    }
}