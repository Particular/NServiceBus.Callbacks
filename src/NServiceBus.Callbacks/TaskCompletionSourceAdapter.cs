namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;

    interface TaskCompletionSourceAdapter
    {
        Type ResponseType { get; }

        void TrySetResult(object result);

        void TrySetCanceled();
    }

    class TaskCompletionSourceAdapter<TResult> : TaskCompletionSourceAdapter
    {
        TaskCompletionSource<TResult> taskCompletionSource;

        public TaskCompletionSourceAdapter(TaskCompletionSource<TResult> tcs)
        {
            taskCompletionSource = tcs;
            ResponseType = typeof(TResult);
        }

        public Type ResponseType { get; }

        public void TrySetResult(object result)
        {
            Task.Run(() => taskCompletionSource.TrySetResult((TResult) result));
        }

        public void TrySetCanceled()
        {
            Task.Run(() => taskCompletionSource.TrySetCanceled());
        }
    }
}