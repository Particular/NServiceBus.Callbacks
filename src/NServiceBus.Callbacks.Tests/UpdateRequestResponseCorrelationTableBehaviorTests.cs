namespace NServiceBus.Callbacks.Tests
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Testing;

    [TestFixture]
    public class UpdateRequestResponseCorrelationTableBehaviorTests
    {
        [Test]
        public async Task Should_not_leak_state_when_request_canceled()
        {
            var dictionary = new ConcurrentDictionary<string, TaskCompletionSourceAdapter>();
            var requestResponseStateLookup = new RequestResponseStateLookup(dictionary);

            var tcs = new TaskCompletionSource<object>();
            var adapter = new TaskCompletionSourceAdapter(tcs);
            var tokenSource = new CancellationTokenSource();
            var outgoingPhysicalMessageContext = new TestableOutgoingPhysicalMessageContext();
            outgoingPhysicalMessageContext.Extensions.Set(new UpdateRequestResponseCorrelationTableBehavior.RequestResponseParameters
            {
                CancellationToken = tokenSource.Token,
                TaskCompletionSource = adapter
            });

            var behavior = new UpdateRequestResponseCorrelationTableBehavior(requestResponseStateLookup);
            await behavior.Invoke(outgoingPhysicalMessageContext, () =>
            {
                tokenSource.Cancel();
                return Task.FromResult(0);
            });

            Assert.ThrowsAsync<TaskCanceledException>(async () => { await tcs.Task; });
            Assert.IsEmpty(dictionary);
        }
    }
}