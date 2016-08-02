namespace NServiceBus
{
    using System;
    using Pipeline;
    using Transport;

    static class IncomingContextExtensions
    {
        public static RequestResponseStateLookup.State? TryRemoveResponseStateBasedOnCorrelationId(this IIncomingContext context, IncomingMessage message, RequestResponseStateLookup lookup)
        {
            var correlationId = context.GetCorrelationId();

            if (correlationId == null)
            {
                return null;
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
                return null;
            }

            RequestResponseStateLookup.State state;
            return lookup.TryRemove(correlationId, out state) ? (RequestResponseStateLookup.State?) state : null;
        }

        static string GetCorrelationId(this IMessageProcessingContext context)
        {
            string str;
            return context.MessageHeaders.TryGetValue(Headers.CorrelationId, out str) ? str : null;
        }

        static Version minimumVersionThatSupportMessageIntent_Reply = new Version(4, 3);
    }
}