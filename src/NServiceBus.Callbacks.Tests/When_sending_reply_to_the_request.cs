namespace NServiceBus.Callbacks.Tests
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Testing;
    using NUnit.Framework;
    using Transport;

    [TestFixture]
    public class When_sending_reply_to_the_request
    {
        [TestCase("6.0.0", MessageIntent.Reply, true)]
        [TestCase("6.0.0", MessageIntent.Send, false)]
        [TestCase("6.0.0", MessageIntent.Publish, false)]
        [TestCase("6.0.0", MessageIntent.Subscribe, false)]
        [TestCase("6.0.0", MessageIntent.Unsubscribe, false)]
        [TestCase("5.0.0", MessageIntent.Reply, true)]
        [TestCase("5.0.0", MessageIntent.Send, false)]
        [TestCase("5.0.0", MessageIntent.Publish, false)]
        [TestCase("5.0.0", MessageIntent.Subscribe, false)]
        [TestCase("5.0.0", MessageIntent.Unsubscribe, false)]
        [TestCase("4.7.0", MessageIntent.Reply, true)]
        [TestCase("4.7.0", MessageIntent.Send, true)]
        [TestCase("4.7.0", MessageIntent.Publish, true)]
        [TestCase("4.7.0", MessageIntent.Subscribe, true)]
        [TestCase("4.7.0", MessageIntent.Unsubscribe, true)]
        public void From_v5_0_0_should_return_value_only_for_reply_intent(string nsbVersion, MessageIntent intent, bool expectedNonEmptyResult)
        {
            var correlationId = new Guid().ToString();
            var lookup = new RequestResponseStateLookup();
            lookup.RegisterState(correlationId, new RequestResponseStateLookup.State());
            var message = new IncomingMessageFromLegacyEndpoint(nsbVersion, intent);
            var incomingContext = new TestableIncomingLogicalMessageContext();
            incomingContext.MessageHeaders.Add(Headers.CorrelationId, correlationId);

            var result = incomingContext.TryRemoveResponseStateBasedOnCorrelationId(message, lookup);

            Assert.AreEqual(expectedNonEmptyResult, result.HasValue);
        }

        [TestCase("4.7.12", MessageIntent.Reply)]
        [TestCase("4.7.12", MessageIntent.Send)]
        [TestCase("4.7.12", MessageIntent.Publish)]
        [TestCase("4.7.12", MessageIntent.Subscribe)]
        [TestCase("4.7.12", MessageIntent.Unsubscribe)]
        [TestCase("4.2.9", MessageIntent.Reply)]
        [TestCase("4.2.9", MessageIntent.Send)]
        [TestCase("4.2.9", MessageIntent.Publish)]
        [TestCase("4.2.9", MessageIntent.Subscribe)]
        [TestCase("4.2.9", MessageIntent.Unsubscribe)]
        [TestCase("4.1.0", MessageIntent.Reply)]
        [TestCase("4.1.0", MessageIntent.Send)]
        [TestCase("3.0.0", MessageIntent.Reply)]
        [TestCase("3.0.0", MessageIntent.Publish)]
        public void Below_v5_0_0_should_return_value_for_all_intents(string nsbVersion, MessageIntent intent)
        {
            var correlationId = new Guid().ToString();
            var lookup = new RequestResponseStateLookup();
            lookup.RegisterState(correlationId, new RequestResponseStateLookup.State());
            var message = new IncomingMessageFromLegacyEndpoint(nsbVersion, intent);
            var incomingContext = new TestableIncomingLogicalMessageContext();
            incomingContext.MessageHeaders.Add(Headers.CorrelationId, correlationId);

            var result = incomingContext.TryRemoveResponseStateBasedOnCorrelationId(message, lookup);

            Assert.That(result.HasValue, Is.True);
        }

        class IncomingMessageFromLegacyEndpoint : IncomingMessage
        {
            public IncomingMessageFromLegacyEndpoint(string nsbVersion, MessageIntent msgIntent)
                : base(
                    new Guid().ToString(),
                    new Dictionary<string, string>
                    {
                        {NServiceBus.Headers.NServiceBusVersion, nsbVersion},
                        {NServiceBus.Headers.MessageIntent, msgIntent.ToString()}
                    },
                    new byte[0])
            {
            }
        }
    }
}