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

            if (context.Extensions.TryGet(out requestResponseState))
            {
                lookup.RegisterState(context.MessageId, requestResponseState);

                requestResponseState.Register(state =>
                {
                    var s = (Tuple<RequestResponseStateLookup, string>)state;
                    var stateLookup = s.Item1;
                    var messageId = s.Item2;

                    RequestResponseStateLookup.State responseState;
                    if (stateLookup.TryRemove(messageId, out responseState))
                    {
                        responseState.TaskCompletionSource.TrySetCanceled();
                    }
                }, Tuple.Create(lookup, context.MessageId));
            }

            return next();
        }

        RequestResponseStateLookup lookup;

        public class Registration : RegisterStep
        {
            public Registration(RequestResponseStateLookup lookup)
                : base("UpdateRequestResponseCorrelationTable", typeof(UpdateRequestResponseCorrelationTableBehavior), "Updates the correlation table that keeps track of synchronous request/response callbacks", b => new UpdateRequestResponseCorrelationTableBehavior(lookup))
            {
                InsertAfterIfExists("MutateOutgoingTransportMessage");
            }
        }
    }
}