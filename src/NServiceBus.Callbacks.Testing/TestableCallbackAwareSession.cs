namespace NServiceBus.Callbacks.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus;
    using NServiceBus.Testing;

    /// <summary>
    /// A callbacks aware testable NServiceBus.IMessageSession implementation.
    /// </summary>
    public class TestableCallbackAwareSession : TestableMessageSession
    {
        List<Tuple<Func<object, SendOptions, bool>, object>> matchers = [];

        /// <summary>
        /// Register a response for thew matched request.
        /// </summary>
        public void When<TRequest, TResult>(Func<TRequest, bool> matcher, TResult response)
            where TRequest : class
        {
            When((TRequest m, SendOptions _) => matcher(m), response);
        }

        /// <summary>
        /// Register a response for thew matched request.
        /// </summary>
        public void When<TRequest, TResult>(Func<TRequest, SendOptions, bool> matcher, TResult response)
            where TRequest : class
        {
            matchers.Add(Tuple.Create<Func<object, SendOptions, bool>, object>((m, o) =>
            {
                return m is TRequest msg && matcher(msg, o);
            }, response));
        }

        /// <summary>
        ///  Sends the provided message.
        /// </summary>
        public override async Task Send(object message, SendOptions options, CancellationToken cancellationToken = default)
        {
            await base.Send(message, options, cancellationToken).ConfigureAwait(false);

            if (options.GetExtensions().TryGet(out RequestResponseStateLookup.State state))
            {
                foreach (var matcher in matchers)
                {
                    TrySetCanceled(state);

                    var result = matcher.Item1(message, options);
                    if (result)
                    {
                        try
                        {
                            state.TaskCompletionSource.TrySetResult(matcher.Item2);
                        }
                        catch (InvalidCastException exception)
                        {
                            throw new InvalidOperationException($"Matcher matched but response type '{matcher.Item2?.GetType()}' is incompatible with expected response type of '{state.TaskCompletionSource.ResponseType}'.", exception);
                        }
                        return;
                    }
                }

                TrySetCanceled(state);
            }
        }

        static void TrySetCanceled(RequestResponseStateLookup.State state)
        {
            if (state.CancellationToken.IsCancellationRequested)
            {
                state.TaskCompletionSource.TrySetCanceled();
            }
        }
    }
}