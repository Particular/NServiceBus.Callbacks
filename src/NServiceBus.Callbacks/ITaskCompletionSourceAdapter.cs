namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;

    interface ITaskCompletionSourceAdapter
    {
        Type ResponseType { get; }

        void TrySetResult(object result);

        void TrySetCanceled();

        void TrySetException(Exception exception);
    }

    class TaskCompletionSourceAdapter<TResult> : ITaskCompletionSourceAdapter
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
            // cast the result type on the invoker's thread
            var castedResult = (TResult)result;
            // prevent the continuation from blocking the pipeline by invoking it in parallel.
            // Consider switching to TaskCreationOptions.RunContinuationsAsynchronously when updating the framework to 4.6. See https://blogs.msdn.microsoft.com/pfxteam/2015/02/02/new-task-apis-in-net-4-6/.
            _ = Task.Run(() => taskCompletionSource.TrySetResult(castedResult));
        }

        public void TrySetCanceled()
        {
            // prevent the continuation from blocking the pipeline by invoking it in parallel.
            // Consider switching to TaskCreationOptions.RunContinuationsAsynchronously when updating the framework to 4.6. See https://blogs.msdn.microsoft.com/pfxteam/2015/02/02/new-task-apis-in-net-4-6/.
            _ = Task.Run(() => taskCompletionSource.TrySetCanceled());
        }

        public void TrySetException(Exception exception)
        {
            // prevent the continuation from blocking the pipeline by invoking it in parallel.
            // Consider switching to TaskCreationOptions.RunContinuationsAsynchronously when updating the framework to 4.6. See https://blogs.msdn.microsoft.com/pfxteam/2015/02/02/new-task-apis-in-net-4-6/.
            _ = Task.Run(() => taskCompletionSource.TrySetException(exception));
        }
    }
}