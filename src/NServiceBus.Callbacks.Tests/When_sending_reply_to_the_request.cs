namespace NServiceBus.Callbacks.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Testing;
    using Transport;

    [TestFixture]
    class When_sending_reply_to_the_request
    {
        [TestCase("5.0.0", MessageIntentEnum.Reply, true)]
        [TestCase("5.0.0", MessageIntentEnum.Send, false)]
        [TestCase("4.3.0", MessageIntentEnum.Reply, true)]
        [TestCase("4.3.0", MessageIntentEnum.Send, false)]
        [TestCase("4.3.0", MessageIntentEnum.Publish, false)]
        [TestCase("4.3.0", MessageIntentEnum.Subscribe, false)]
        [TestCase("4.3.0", MessageIntentEnum.Unsubscribe, false)]
        public void From_v4_3_0_should_return_value_only_for_reply_intent(string nsbVersion, MessageIntentEnum intent, bool expectedNonEmptyResult)
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

        [TestCase("4.2.9", MessageIntentEnum.Reply)]
        [TestCase("4.2.9", MessageIntentEnum.Send)]
        [TestCase("4.2.9", MessageIntentEnum.Publish)]
        [TestCase("4.2.9", MessageIntentEnum.Subscribe)]
        [TestCase("4.2.9", MessageIntentEnum.Unsubscribe)]
        [TestCase("4.1.0", MessageIntentEnum.Reply)]
        [TestCase("4.1.0", MessageIntentEnum.Send)]
        [TestCase("3.0.0", MessageIntentEnum.Reply)]
        [TestCase("3.0.0", MessageIntentEnum.Publish)]
        public void Below_v4_3_0_should_return_value_for_all_intents(string nsbVersion, MessageIntentEnum intent)
        {
            var correlationId = new Guid().ToString();
            var lookup = new RequestResponseStateLookup();
            lookup.RegisterState(correlationId, new RequestResponseStateLookup.State());
            var message = new IncomingMessageFromLegacyEndpoint(nsbVersion, intent);
            var incomingContext = new TestableIncomingLogicalMessageContext();
            incomingContext.MessageHeaders.Add(Headers.CorrelationId, correlationId);

            var result = incomingContext.TryRemoveResponseStateBasedOnCorrelationId(message, lookup);

            Assert.IsTrue(result.HasValue);
        }

        class IncomingMessageFromLegacyEndpoint : IncomingMessage
        {
            public IncomingMessageFromLegacyEndpoint(string nsbVersion, MessageIntentEnum msgIntent)
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