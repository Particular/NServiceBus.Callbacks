namespace NServiceBus
{
    using System.Threading;
    using Extensibility;

    static class ExtendableOptionsExtensions
    {
        public static void RegisterCancellationToken(this ExtendableOptions options, CancellationToken cancellationToken = default)
        {
            var extensions = options.GetExtensions();
            if (extensions.TryGet(out RequestResponseStateLookup.State state))
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