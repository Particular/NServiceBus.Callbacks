namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;

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

                    Assert.That(c.HandlerGotTheRequest, Is.True);
                    c.CallbackFired = true;
                }))
                .Done(c => c.CallbackFired)
                .Run();

            Assert.Multiple(() =>
            {
                Assert.That(context.CallbackFired, Is.True);
                Assert.That(context.HandlerGotTheRequest, Is.True);
            });
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
                    Assert.That(testContext.CallbackFired, Is.False);
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