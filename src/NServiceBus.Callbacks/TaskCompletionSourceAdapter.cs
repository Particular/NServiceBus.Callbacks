namespace NServiceBus
{
    using System;
    using System.Threading;

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

        public void TrySetCanceled()
        {
            var method = taskCompletionSource.GetType().GetMethod("TrySetCanceled", TrySetCancelledArgumentTypes);
            method.Invoke(taskCompletionSource, TrySetCancelledArguments);
        }

        object taskCompletionSource;

        static Type[] TrySetCancelledArgumentTypes = {
            typeof(CancellationToken)
        };

        static object[] TrySetCancelledArguments =
        {
            CancellationToken.None
        };
    }
}