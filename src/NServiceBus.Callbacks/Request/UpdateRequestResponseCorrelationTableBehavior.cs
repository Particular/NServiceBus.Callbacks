namespace NServiceBus
{
    using System;
    using System.Threading;
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
            RequestResponseParameters parameters;

            if (context.Extensions.TryGet(out parameters) && !parameters.CancellationToken.IsCancellationRequested)
            {
                parameters.Register(state =>
                {
                    var s = (Tuple<RequestResponseStateLookup, string>) state;
                    var stateLookup = s.Item1;
                    var messageId = s.Item2;

                    TaskCompletionSourceAdapter tcs;
                    if (stateLookup.TryGet(messageId, out tcs))
                    {
                        tcs.TrySetCanceled();
                    }
                }, Tuple.Create(lookup, context.MessageId));
                lookup.RegisterState(context.MessageId, parameters.TaskCompletionSource);
            }

            return next();
        }

        RequestResponseStateLookup lookup;

        public struct RequestResponseParameters : IDisposable
        {
            public void Dispose()
            {
                Registrations.Dispose();
            }

            public void Register(Action<object> action, object state)
            {
                Registrations = CancellationToken.Register(action, state);
            }

            public CancellationToken CancellationToken;
            public TaskCompletionSourceAdapter TaskCompletionSource;
            CancellationTokenRegistration Registrations;
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