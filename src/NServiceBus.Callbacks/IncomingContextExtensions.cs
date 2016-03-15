namespace NServiceBus
{
    using System;
    using Pipeline;
    using Transports;

    static class IncomingContextExtensions
    {
        public static CorrelationIdAndTaskCompletionSource GetCorrelationIdAndCompletionSource(this IIncomingContext context, IncomingMessage message, RequestResponseStateLookup lookup)
        {
            var correlationId = context.GetCorrelationId();

            var emptyResult = new CorrelationIdAndTaskCompletionSource();
            if (correlationId == null)
            {
                return emptyResult;
            }

            string version;
            var checkMessageIntent = true;

            if (message.Headers.TryGetValue(Headers.NServiceBusVersion, out version))
            {
                Version parsedVersion;
                if (Version.TryParse(version, out parsedVersion))
                {
                    if (parsedVersion < minimumVersionThatSupportMessageIntent_Reply)
                    {
                        checkMessageIntent = false;
                    }
                }
            }

            var messageIntentEnum = message.GetMesssageIntent();
            if (checkMessageIntent && messageIntentEnum != MessageIntentEnum.Reply)
            {
                return emptyResult;
            }

            TaskCompletionSourceAdapter tcs;
            return !lookup.TryGet(correlationId, out tcs) ? emptyResult : new CorrelationIdAndTaskCompletionSource(correlationId, tcs);
        }

        static string GetCorrelationId(this IMessageProcessingContext context)
        {
            string str;
            return context.MessageHeaders.TryGetValue(Headers.CorrelationId, out str) ? str : null;
        }

        static Version minimumVersionThatSupportMessageIntent_Reply = new Version(4, 3);

        internal class CorrelationIdAndTaskCompletionSource
        {
            public CorrelationIdAndTaskCompletionSource()
            {
                HasValue = false;
            }

            public CorrelationIdAndTaskCompletionSource(string correlationId, TaskCompletionSourceAdapter taskCompletionSource)
            {
                CorrelationId = correlationId;
                TaskCompletionSource = taskCompletionSource;
                HasValue = true;
            }

            public bool HasValue { get; }

            public string CorrelationId { get; }

            public TaskCompletionSourceAdapter TaskCompletionSource { get; }
        }
    }
}