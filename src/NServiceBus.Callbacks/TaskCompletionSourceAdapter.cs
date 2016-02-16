namespace NServiceBus
{
    using System;

    class TaskCompletionSourceAdapter
    {
        public TaskCompletionSourceAdapter(object taskCompletionSource)
        {
            this.taskCompletionSource = taskCompletionSource;
        }

        public Type ResponseType => taskCompletionSource.GetType().GenericTypeArguments[0];

        public void TrySetResult(object result)
        {
            var method = taskCompletionSource.GetType().GetMethod("TrySetResult");
            method.Invoke(taskCompletionSource, new[]
            {
                result
            });
        }

        public void SetCancelled()
        {
            var method = taskCompletionSource.GetType().GetMethod("SetCanceled");
            method.Invoke(taskCompletionSource, new object[]
            {
            });
        }

        object taskCompletionSource;
    }
}