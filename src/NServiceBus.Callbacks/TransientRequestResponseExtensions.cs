namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;

    /// <summary>
    ///  Request/response extension methods.
    /// </summary>
    public static class TransientRequestResponseExtensions
    {
        /// <summary>
        /// Sends a <paramref name="requestMessage"/> to the configured destination and returns back a <see cref="Task{TResponse}"/> which can be awaited.
        /// </summary>
        /// <remarks> The task returned is non durable. When the AppDomain is unloaded or the response task is canceled. 
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        ///  that response message and therefore the message will be moved to the error queue.</remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="bus">Object being extended.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="options">The options for the send.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static Task<TResponse> RequestWithTransientlyHandledResponse<TResponse>(this IBus bus, object requestMessage, SendOptions options)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException("requestMessage");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (bus == null)
            {
                throw new ArgumentNullException("bus");
            }

            options.SetHeader("$Routing.RouteReplyToSpecificEndpointInstance", Boolean.TrueString);

            if (typeof(TResponse).IsIntOrEnum())
            {
                var tcs = new TaskCompletionSource<CallbackResponse<TResponse>>();

                var adapter = new TaskCompletionSourceAdapter(tcs);
                options.RegisterTokenSource(adapter);
                var continuationTask = tcs.Task.ContinueWith(t => t.Result.Status, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);

                bus.Send(requestMessage, options);

                return continuationTask;
            }
            else
            {
                var tcs = new TaskCompletionSource<TResponse>();

                var adapter = new TaskCompletionSourceAdapter(tcs);
                options.RegisterTokenSource(adapter);

                bus.Send(requestMessage, options);

                return tcs.Task;
            }
        }

        
        static void RegisterTokenSource(this ExtendableOptions options, TaskCompletionSourceAdapter adapter)
        {
            var extensions = options.GetExtensions();
            UpdateRequestResponseCorrelationTableBehavior.RequestResponseParameters data;
            if (extensions.TryGet(out data))
            {
                data.TaskCompletionSource = adapter;
            }
            else
            {
                data = new UpdateRequestResponseCorrelationTableBehavior.RequestResponseParameters { TaskCompletionSource = adapter };
                extensions.Set(data);
            }
        }
    }
}
