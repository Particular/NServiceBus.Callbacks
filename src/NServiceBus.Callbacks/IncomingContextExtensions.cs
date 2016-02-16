namespace NServiceBus
{
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

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
                if (version.StartsWith("3."))
                {
                    checkMessageIntent = false;
                }
            }

            if (checkMessageIntent && message.GetMesssageIntent() != MessageIntentEnum.Reply)
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