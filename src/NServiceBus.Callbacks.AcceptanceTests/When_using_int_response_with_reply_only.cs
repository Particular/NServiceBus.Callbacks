namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using NUnit.Framework;

    public class When_using_int_response_with_reply_only : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_response_and_not_require_uniquely_addressable()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    c.Response = await bus.Request<int>(new MyRequest(), new SendOptions());
                    c.CallbackFired = true;
                }))
                .WithEndpoint<Replier>()
                .Done(c => c.CallbackFired)
                .Run();

            Assert.AreEqual(200, context.Response);
        }

        class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
            public int Response { get; set; }
        }

        class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c =>
                    c.Callbacks().EnableCallbackRepliesOnly());
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(200);
                }
            }
        }

        class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                        c.MakeInstanceUniquelyAddressable("1"))
                    .AddMapping<MyRequest>(typeof(Replier));
            }
        }

        public class MyRequest : IMessage
        {
        }
    }
}