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
        public void When_request_option_matcher_matches_with_wrong_response_type()
        {
            var callbackSession = new TestableCallbackAwareSession();
            var expectedHeader = "TestingHeader";

            callbackSession.When<Request, int>((r, o) => r.MagicNumber == 42 && o.GetHeaders().ContainsKey(expectedHeader), 42);

            var options = new SendOptions();
            options.SetHeader(expectedHeader, "value");

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await callbackSession.Request<string>(new Request { MagicNumber = 42 }, options));
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
        public async Task When_request_option_matcher_matches_should_return_response()
        {
            var callbackSession = new TestableCallbackAwareSession();
            var expectedHeader = "TestingHeader";

            callbackSession.When<Request, string>((r, o) => r.MagicNumber == 42 && o.GetHeaders().ContainsKey(expectedHeader), "HelloWorld");

            var options = new SendOptions();
            options.SetHeader(expectedHeader, "value");

            var result = await callbackSession.Request<string>(new Request { MagicNumber = 42 }, options);

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
        public void When_request_matcher_matches_for_cancellation()
        {
            var tokenSource = new CancellationTokenSource();

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>(r =>
                                                  {
                                                      return r.MagicNumber == 42 ? TestableCallbackAwareSession.MatchState.Cancel : TestableCallbackAwareSession.MatchState.NoMatch;
                                                  }, "HelloWorld");

            Assert.ThrowsAsync<TaskCanceledException>(async () => await callbackSession.Request<string>(new Request { MagicNumber = 42 }, tokenSource.Token));
        }

        [Test]
        public void When_request_option_matcher_matches_for_cancellation()
        {
            var tokenSource = new CancellationTokenSource();
            var expectedHeader = "TestingHeader";

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>((r, o) =>
                                                  {
                                                      return r.MagicNumber == 42 && o.GetHeaders().ContainsKey(expectedHeader) ? TestableCallbackAwareSession.MatchState.Cancel : TestableCallbackAwareSession.MatchState.NoMatch;
                                                  }, "HelloWorld");

            var options = new SendOptions();
            options.SetHeader(expectedHeader, "value");

            Assert.ThrowsAsync<TaskCanceledException>(async () => await callbackSession.Request<string>(new Request { MagicNumber = 42 }, options, tokenSource.Token));
        }

        [Test]
        public void When_request_option_matcher_does_not_match_should_honor_cancellation()
        {
            var tokenSource = new CancellationTokenSource();
            var expectedHeader = "TestingHeader";

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>((r, o) =>
            {
                tokenSource.Cancel();
                return r.MagicNumber == 42 && o.GetHeaders().ContainsKey("WrongHeader");
            }, "HelloWorld");

            var options = new SendOptions();
            options.SetHeader(expectedHeader, "value");

            Assert.ThrowsAsync<TaskCanceledException>(async () => await callbackSession.Request<string>(new Request { MagicNumber = 42 }, options, tokenSource.Token));
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

        [Test]
        public async Task When_request_option_matcher_matches_should_not_honor_cancellation()
        {
            var tokenSource = new CancellationTokenSource();
            var expectedHeader = "TestingHeader";

            var callbackSession = new TestableCallbackAwareSession();

            callbackSession.When<Request, string>((r,o) =>
            {
                tokenSource.Cancel();
                return r.MagicNumber == 42 && o.GetHeaders().ContainsKey(expectedHeader);
            }, "HelloWorld");

            var options = new SendOptions();
            options.SetHeader(expectedHeader, "value");

            var result = await callbackSession.Request<string>(new Request { MagicNumber = 42 }, options, tokenSource.Token);

            Assert.AreEqual("HelloWorld", result);
        }

        class Request
        {
            public int MagicNumber { get; set; }
        }
    }
}
