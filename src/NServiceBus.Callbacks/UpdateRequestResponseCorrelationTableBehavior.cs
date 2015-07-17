﻿namespace NServiceBus
{
    using System;
    using System.Threading;
    using NServiceBus.Callbacks;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Pipeline;

    class UpdateRequestResponseCorrelationTableBehavior : PhysicalOutgoingContextStageBehavior
    {
        RequestResponseStateLookup lookup;

        public UpdateRequestResponseCorrelationTableBehavior(RequestResponseStateLookup lookup)
        {
            this.lookup = lookup;
        }

        public override void Invoke(Context context, Action next)
        {
            RequestResponseParameters parameters;

            if (context.TryGet(out parameters) && !parameters.CancellationToken.IsCancellationRequested)
            {
                var messageId = context.GetMessageId();
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

            next();
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