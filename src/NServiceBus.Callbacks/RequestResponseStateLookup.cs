namespace NServiceBus
{
    using System.Collections.Concurrent;

    class RequestResponseStateLookup
    {
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

        ConcurrentDictionary<string, TaskCompletionSourceAdapter> messageIdToCompletionSource = new ConcurrentDictionary<string, TaskCompletionSourceAdapter>();
    }
}