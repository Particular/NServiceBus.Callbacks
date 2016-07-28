namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;

    class UpdateRequestResponseCorrelationTableBehavior : Behavior<IOutgoingPhysicalMessageContext>
    {
        public UpdateRequestResponseCorrelationTableBehavior(RequestResponseStateLookup lookup)
        {
            this.lookup = lookup;
        }

        public override Task Invoke(IOutgoingPhysicalMessageContext context, Func<Task> next)
        {
            RequestResponseStateLookup.State requestResponseState;

            if (context.Extensions.TryGet(out requestResponseState) && !requestResponseState.CancellationToken.IsCancellationRequested)
            {
                requestResponseState.Register(state =>
                {
                    var s = (Tuple<RequestResponseStateLookup, string>) state;
                    var stateLookup = s.Item1;
                    var messageId = s.Item2;

                    RequestResponseStateLookup.State responseState;
                    if (stateLookup.TryRemove(messageId, out responseState))
                    {
                        responseState.TaskCompletionSource.TrySetCanceled();
                    }
                }, Tuple.Create(lookup, context.MessageId));
                lookup.RegisterState(context.MessageId, requestResponseState);
            }

            return next();
        }

        RequestResponseStateLookup lookup;

        public class Registration : RegisterStep
        {
            public Registration()
                : base("UpdateRequestResponseCorrelationTable", typeof(UpdateRequestResponseCorrelationTableBehavior), "Updates the correlation table that keeps track of synchronous request/response callbacks")
            {
                InsertAfterIfExists("MutateOutgoingTransportMessage");
            }
        }
    }
}