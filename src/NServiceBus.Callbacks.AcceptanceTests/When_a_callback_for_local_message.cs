namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_a_callback_for_local_message : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_trigger_the_callback_when_the_response_comes_back()
        {
            await Scenario.Define<Context>()
                    .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, context) =>
                        {
                            var options = new SendOptions();

                            options.RouteToLocalEndpointInstance();

                            await bus.Request<MyResponse>(new MyRequest(), options);
                            
                            Assert.True(context.HandlerGotTheRequest);
                            context.CallbackFired = true;
                        }))
                    .Done(c => c.CallbackFired)
                    .Repeat(r => r.For(Transports.Default))
                    .Should(c =>
                    {
                        Assert.True(c.CallbackFired);
                        Assert.True(c.HandlerGotTheRequest);
                    })
                    .Run();
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
                EndpointSetup<DefaultServer>();
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Assert.False(Context.CallbackFired);
                    Context.HandlerGotTheRequest = true;

                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage { }

        public class MyResponse : IMessage { }
    }
}