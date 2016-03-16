namespace NServiceBus.Callbacks.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Extensibility;
    using NUnit.Framework;
    using ObjectBuilder;
    using Pipeline;

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
            lookup.RegisterState(correlationId, new TaskCompletionSourceAdapter(new object()));
            Transports.IncomingMessage message = new IncomingMessage(nsbVersion, intent);
            var incomingContext = new IncomingContext(correlationId);

            var result = incomingContext.GetCorrelationIdAndCompletionSource(message, lookup);

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
            lookup.RegisterState(correlationId, new TaskCompletionSourceAdapter(new object()));
            Transports.IncomingMessage message = new IncomingMessage(nsbVersion, intent);
            var incomingContext = new IncomingContext(correlationId);

            var result = incomingContext.GetCorrelationIdAndCompletionSource(message, lookup);

            Assert.IsTrue(result.HasValue);
        }

        class IncomingMessage : Transports.IncomingMessage
        {
            public IncomingMessage(string nsbVersion, MessageIntentEnum msgIntent) 
                : base(
                      new Guid().ToString(), 
                      new Dictionary<string, string> {
                        { NServiceBus.Headers.NServiceBusVersion, nsbVersion },
                        { NServiceBus.Headers.MessageIntent, msgIntent.ToString() }}, 
                      new MemoryStream()) { }
        }

        //TODO: replace this class when mocks are provided for context
        //https://github.com/Particular/NServiceBus.Testing/issues/29
        class IncomingContext : IIncomingContext
        {
            public IncomingContext(string correlationId) : 
                this(new Dictionary<string, string>{ {Headers.CorrelationId, correlationId} })
            { 
            }

            IncomingContext(IReadOnlyDictionary<string, string> headers)
            {
                MessageHeaders = headers;
            }

            public IReadOnlyDictionary<string, string> MessageHeaders { get; }
            
            public ContextBag Extensions { get; }
            public IBuilder Builder { get; }
            public Task Send(object message, SendOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Send<T>(Action<T> messageConstructor, SendOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, PublishOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
            {
                throw new NotImplementedException();
            }

            public Task Reply(object message, ReplyOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
            {
                throw new NotImplementedException();
            }

            public Task ForwardCurrentMessageTo(string destination)
            {
                throw new NotImplementedException();
            }

            public string MessageId { get; }
            public string ReplyToAddress { get; }
        }
    }
}
