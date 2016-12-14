namespace NServiceBus.Callback.Testing.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Callbacks.Testing;
    using NUnit.Framework;

    [TestFixture]
    public class TestablecallbackAwareSessionTests
    {
        [Test]
        public void When_request_matcher_matches_with_wrong_response_type()
        {
            var callbackSession = new TestableCallbackAwareSession();
            callbackSession.When<Request, int>(r => r.MagicNumber == 42, 42);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async() => await callbackSession.Request<string>(new Request { MagicNumber = 42 }));
            StringAssert.Contains("Matcher matched but response type 'System.Int32' is incompatible with expected response type of 'System.String'.", exception.Message);
        }

        [Test]
        public async Task When_request_matcher_matches_should_return_response()
        {
            var callbackSession = new TestableCallbackAwareSession();
            callbackSession.When<Request, string>(r => r.MagicNumber == 42, "HelloWorld");

            var result = await callbackSession.Request<string>(new Request { MagicNumber = 42 });

            Assert.AreEqual("HelloWorld", result);
        }

        [Test]
        public void When_request_matcher_does_not_match_should_honor_cancellation()
        {
            var tokenSource = new CancellationTokenSource();

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>(r =>
            {
                tokenSource.Cancel();
                return r.MagicNumber == 43;
            }, "HelloWorld");

            Assert.ThrowsAsync<TaskCanceledException>(async () => await callbackSession.Request<string>(new Request { MagicNumber = 42 }, tokenSource.Token));
        }

        [Test]
        public async Task When_request_matcher_matches_should_not_honor_cancellation()
        {
            var tokenSource = new CancellationTokenSource();

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>(r =>
            {
                tokenSource.Cancel();
                return r.MagicNumber == 42;
            }, "HelloWorld");

            var result = await callbackSession.Request<string>(new Request { MagicNumber = 42 }, tokenSource.Token);

            Assert.AreEqual("HelloWorld", result);
        }

        class Request
        {
            public int MagicNumber { get; set; }
        }
    }
}
