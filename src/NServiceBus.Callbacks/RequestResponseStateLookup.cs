namespace NServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    class RequestResponseStateLookup
    {
        public RequestResponseStateLookup() : this(new ConcurrentDictionary<string, State>())
        {
        }

        // For testing purposes
        internal RequestResponseStateLookup(ConcurrentDictionary<string, State> dictionary)
        {
            messageIdToCompletionSource = dictionary;
        }

        public void RegisterState(string messageId, State state)
        {
            messageIdToCompletionSource[messageId] = state;
        }

        public bool TryRemove(string messageId, out State state)
        {
            return messageIdToCompletionSource.TryRemove(messageId, out state);
        }

        ConcurrentDictionary<string, State> messageIdToCompletionSource;

        public struct State : IDisposable
        {
            public void Dispose()
            {
                Registrations.Dispose();
            }

            public void Register(Action<object> action, object state)
            {
                Registrations = CancellationToken.Register(action, state);
            }

            public CancellationToken CancellationToken;
            public ITaskCompletionSourceAdapter TaskCompletionSource;
            CancellationTokenRegistration Registrations;
        }
    }
}