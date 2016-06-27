namespace NServiceBus
{
    using System.Collections.Concurrent;

    class RequestResponseStateLookup
    {
        public RequestResponseStateLookup() : this(new ConcurrentDictionary<string, TaskCompletionSourceAdapter>())
        {
        }

        // For testing purposes
        internal RequestResponseStateLookup(ConcurrentDictionary<string, TaskCompletionSourceAdapter> dictionary)
        {
            messageIdToCompletionSource = dictionary;
        }

        public void RegisterState(string messageId, TaskCompletionSourceAdapter state)
        {
            messageIdToCompletionSource[messageId] = state;
        }

        public bool TryGet(string messageId, out TaskCompletionSourceAdapter state)
        {
            return messageIdToCompletionSource.TryGetValue(messageId, out state);
        }

        public void RemoveState(string messageId)
        {
            TaskCompletionSourceAdapter state;
            messageIdToCompletionSource.TryRemove(messageId, out state);
        }

        ConcurrentDictionary<string, TaskCompletionSourceAdapter> messageIdToCompletionSource;
    }
}