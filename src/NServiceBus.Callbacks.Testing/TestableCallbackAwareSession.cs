namespace NServiceBus.Callbacks.Testing
{
    using System;
    using System.Collections.Generic;
    using NServiceBus;
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus.Testing;

    public class TestableCallbackAwareSession : TestableMessageSession
    {
        public enum MatchState
        {
            Match,
            NoMatch,
            Cancel
        }
        List<Tuple<Func<object, SendOptions, MatchState>, object>> matchers = new List<Tuple<Func<object, SendOptions, MatchState>, object>>();

        public void When<TRequest, TResult>(Func<TRequest, bool> matcher, TResult response)
            where TRequest : class
        {
            When((TRequest m, SendOptions _) => matcher(m), response);
        }

        public void When<TRequest, TResult>(Func<TRequest, MatchState> matcher, TResult response)
            where TRequest : class
        {
            When((TRequest m, SendOptions _) => matcher(m), response);
        }

        public void When<TRequest, TResult>(Func<TRequest, SendOptions, bool> matcher, TResult response)
            where TRequest : class
        {
            matchers.Add(Tuple.Create<Func<object, SendOptions, MatchState>, object>((m, o) =>
            {
                var msg = m as TRequest;
                return msg != null && matcher(msg, o) ? MatchState.Match : MatchState.NoMatch;
            }, response));
        }

        public void When<TRequest, TResult>(Func<TRequest, SendOptions, MatchState> matcher, TResult response)
            where TRequest : class
        {
            matchers.Add(Tuple.Create<Func<object, SendOptions, MatchState>, object>((m, o) =>
            {
                var msg = m as TRequest;
                if (msg == null) return MatchState.NoMatch;
                return matcher(msg, o);
            }, response));
        }

        public override async Task Send(object message, SendOptions options)
        {
            await base.Send(message, options).ConfigureAwait(false);

            RequestResponseStateLookup.State state;
            if (options.GetExtensions().TryGet(out state))
            {
                foreach (var matcher in matchers)
                {
                    TrySetCanceled(state);

                    var result = matcher.Item1(message, options);
                    switch (result)
                    {
                        case MatchState.Match:
                            try
                            {
                                state.TaskCompletionSource.TrySetResult(matcher.Item2);
                            }
                            catch (InvalidCastException exception)
                            {
                                throw new InvalidOperationException($"Matcher matched but response type '{matcher.Item2?.GetType()}' is incompatible with expected response type of '{state.TaskCompletionSource.ResponseType}'.", exception);
                            }

                            return;
                        case MatchState.Cancel:
                            state.TaskCompletionSource.TrySetCanceled();
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