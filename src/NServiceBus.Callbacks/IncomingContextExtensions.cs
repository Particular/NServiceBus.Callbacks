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

            var checkMessageIntent = true;

            if (message.Headers.TryGetValue(Headers.NServiceBusVersion, out string version))
            {
                if (Version.TryParse(version, out Version parsedVersion))
                {
                    if (parsedVersion < minimumVersionThatSupportMessageIntent_Reply)
                    {
                        checkMessageIntent = false;
                    }
                }
            }

            var messageIntent = message.GetMessageIntent();
            if (checkMessageIntent && messageIntent != MessageIntent.Reply)
            {
                return null;
            }

            return lookup.TryRemove(correlationId, out RequestResponseStateLookup.State state) ? state : null;
        }

        static string GetCorrelationId(this IMessageProcessingContext context)
        {
            return context.MessageHeaders.TryGetValue(Headers.CorrelationId, out string str) ? str : null;
        }

        static Version minimumVersionThatSupportMessageIntent_Reply = new Version(5, 0);
    }
}
