namespace NServiceBus
{
    using NServiceBus.Pipeline.Contexts;

    internal static class IncomingLogicalMessageContextExtensions
    {
        public static string GetCorrelationId(this IncomingContext context)
        {
            string str;
            return context.MessageHeaders.TryGetValue(Headers.CorrelationId, out str) ? str : null;
        }
    }
}