namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_not_routing_replies_to_specific_instance : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_require_unique_instance_address()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithNoUniqueAddress>(e => e.When(async (s, c) =>
                {
                    var options = new SendOptions();

                    options.RouteReplyToAnyInstance();
                    options.RouteToThisEndpoint();

                    await s.Request<MyResponse>(new MyRequest(), options);

                    c.GotResponse = true;
                }))
                .Done(c => c.GotResponse)
                .Run();

            Assert.True(context.GotResponse);
        }

        public class Context : ScenarioContext
        {
            public bool GotResponse { get; set; }
        }

        public class EndpointWithNoUniqueAddress : EndpointConfigurationBuilder
        {
            public EndpointWithNoUniqueAddress()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.EnableCallbacks();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage
        {
        }

        public class MyResponse : IMessage
        {
        }
    }
}