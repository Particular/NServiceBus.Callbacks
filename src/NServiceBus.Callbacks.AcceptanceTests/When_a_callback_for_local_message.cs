namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTesting.EndpointTemplates;

    public class When_a_callback_for_local_message : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_trigger_the_callback_when_the_response_comes_back()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    var options = new SendOptions();

                    options.RouteToThisEndpoint();

                    await bus.Request<MyResponse>(new MyRequest(), options);

                    Assert.True(c.HandlerGotTheRequest);
                    c.CallbackFired = true;
                }))
                .Done(c => c.CallbackFired)
                .Run();

            Assert.True(context.CallbackFired);
            Assert.True(context.HandlerGotTheRequest);
        }

        public class Context : ScenarioContext
        {
            public bool HandlerGotTheRequest { get; set; }

            public bool CallbackFired { get; set; }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.MakeInstanceUniquelyAddressable("1");
                    c.EnableCallbacks();
                });
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                Context testContext;

                public MyRequestHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Assert.False(testContext.CallbackFired);
                    testContext.HandlerGotTheRequest = true;

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