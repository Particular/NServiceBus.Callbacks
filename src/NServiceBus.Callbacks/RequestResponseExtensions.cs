namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;

    /// <summary>
    ///  Request/response extension methods.
    /// </summary>
    public static class RequestResponseExtensions
    {
        /// <summary>
        /// Sends a <paramref name="requestMessage"/> to the configured destination and returns back a <see cref="Task{TResponse}"/> which can be awaited.
        /// </summary>
        /// <remarks> The task returned is non durable. When the AppDomain is unloaded or the response task is canceled. 
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        ///  that response message and therefore the message will be moved to the error queue.</remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="context">Object being extended.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="options">The options for the send.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static async Task<TResponse> Request<TResponse>(this IBusContext context, object requestMessage, SendOptions options)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            options.SetHeader("$Routing.RouteReplyToSpecificEndpointInstance", bool.TrueString);

            var tcs = new TaskCompletionSource<TResponse>();

            var adapter = new TaskCompletionSourceAdapter(tcs);
            options.RegisterTokenSource(adapter);

            await context.Send(requestMessage, options).ConfigureAwait(false);
            
            return await tcs.Task.ConfigureAwait(false);
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
