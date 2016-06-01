namespace NServiceBus
{
    using System.Threading;
    using Extensibility;

    static class ExtendableOptionsExtensions
    {
        public static void RegisterCancellationToken(this ExtendableOptions options, CancellationToken cancellationToken)
        {
            var extensions = options.GetExtensions();
            UpdateRequestResponseCorrelationTableBehavior.RequestResponseParameters data;
            if (extensions.TryGet(out data))
            {
                data.CancellationToken = cancellationToken;
            }
            else
            {
                data = new UpdateRequestResponseCorrelationTableBehavior.RequestResponseParameters
                {
                    CancellationToken = cancellationToken
                };
                extensions.Set(data);
            }
        }
    }
}