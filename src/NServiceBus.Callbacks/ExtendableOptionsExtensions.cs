namespace NServiceBus
{
    using System.Threading;
    using Extensibility;

    static class ExtendableOptionsExtensions
    {
        public static void RegisterCancellationToken(this ExtendableOptions options, CancellationToken cancellationToken)
        {
            var extensions = options.GetExtensions();
            RequestResponseStateLookup.State state;
            if (extensions.TryGet(out state))
            {
                state.CancellationToken = cancellationToken;
            }
            else
            {
                state = new RequestResponseStateLookup.State
                {
                    CancellationToken = cancellationToken
                };
            }
            extensions.Set(state);
        }
    }
}