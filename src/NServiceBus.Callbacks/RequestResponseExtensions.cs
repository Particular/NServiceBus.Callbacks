namespace NServiceBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;

    /// <summary>
    /// Request/response extension methods.
    /// </summary>
    public static class RequestResponseExtensions
    {
        /// <summary>
        /// Sends a <paramref name="requestMessage" /> to the configured destination and returns back a
        /// <see cref="Task{TResponse}" /> which can be awaited.
        /// </summary>
        /// <remarks>
        /// The task returned is non durable. When the AppDomain is unloaded or the response task is canceled.
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        /// that response message and therefore the message will be moved to the error queue.
        /// </remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static Task<TResponse> Request<TResponse>(this IMessageSession session, object requestMessage)
        {
            return session.Request<TResponse>(requestMessage, CancellationToken.None);
        }

        /// <summary>
        /// Sends a <paramref name="requestMessage" /> to the configured destination and returns back a
        /// <see cref="Task{TResponse}" /> which can be awaited.
        /// </summary>
        /// <remarks>
        /// The task returned is non durable. When the AppDomain is unloaded or the response task is canceled.
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        /// that response message and therefore the message will be moved to the error queue.
        /// </remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the request.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static Task<TResponse> Request<TResponse>(this IMessageSession session, object requestMessage, CancellationToken cancellationToken)
        {
            return session.Request<TResponse>(requestMessage, new SendOptions(), cancellationToken);
        }

        /// <summary>
        /// Sends a <paramref name="requestMessage" /> to the configured destination and returns back a
        /// <see cref="Task{TResponse}" /> which can be awaited.
        /// </summary>
        /// <remarks>
        /// The task returned is non durable. When the AppDomain is unloaded or the response task is canceled.
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        /// that response message and therefore the message will be moved to the error queue.
        /// </remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="options">The options for the send.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static Task<TResponse> Request<TResponse>(this IMessageSession session, object requestMessage, SendOptions options)
        {
            return session.Request<TResponse>(requestMessage, options, CancellationToken.None);
        }

        /// <summary>
        /// Sends a <paramref name="requestMessage" /> to the configured destination and returns back a
        /// <see cref="Task{TResponse}" /> which can be awaited.
        /// </summary>
        /// <remarks>
        /// The task returned is non durable. When the AppDomain is unloaded or the response task is canceled.
        /// Messages can still arrive to the requesting endpoint but in that case no handling code will be attached to consume
        /// that response message and therefore the message will be moved to the error queue.
        /// </remarks>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="options">The options for the send.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the request.</param>
        /// <returns>A task which contains the response when it is completed.</returns>
        public static async Task<TResponse> Request<TResponse>(this IMessageSession session, object requestMessage, SendOptions options, CancellationToken cancellationToken)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<TResponse>();
            var adapter = new TaskCompletionSourceAdapter<TResponse>(tcs);
            options.RouteReplyToThisInstance();
            options.RegisterCancellationToken(cancellationToken);

            using (options.RegisterTokenSource(adapter))
            {
                await session.Send(requestMessage, options).ConfigureAwait(false);

                return await tcs.Task.ConfigureAwait(false);
            }
        }


        static RequestResponseStateLookup.State RegisterTokenSource(this ExtendableOptions options, TaskCompletionSourceAdapter adapter)
        {
            var extensions = options.GetExtensions();
            if (extensions.TryGet(out RequestResponseStateLookup.State state))
            {
                state.TaskCompletionSource = adapter;
            }
            else
            {
                state = new RequestResponseStateLookup.State
                {
                    TaskCompletionSource = adapter,
                    CancellationToken = CancellationToken.None
                };
            }
            extensions.Set(state);
            return state;
        }
    }
}