namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

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
                var messageId = context.MessageId;
                parameters.Register(() =>
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

        RequestResponseStateLookup lookup;

        public class RequestResponseParameters : IDisposable
        {
            public RequestResponseParameters()
            {
                CancellationToken = CancellationToken.None;
            }

            public void Dispose()
            {
                foreach (var registration in registrations)
                {
                    registration.Dispose();
                }
            }

            public void Register(Action action)
            {
                registrations.Add(CancellationToken.Register(action));
            }

            public CancellationToken CancellationToken;

            public TaskCompletionSourceAdapter TaskCompletionSource;
            List<CancellationTokenRegistration> registrations = new List<CancellationTokenRegistration>();
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