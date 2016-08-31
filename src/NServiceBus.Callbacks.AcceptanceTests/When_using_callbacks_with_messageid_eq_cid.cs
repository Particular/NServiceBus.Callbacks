namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_using_callbacks_with_messageid_eq_cid : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_trigger_the_callback()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    var id = Guid.NewGuid().ToString();
                    var options = new SendOptions();

                    options.SetMessageId(id);
                    options.RouteToThisEndpoint();

                    await bus.Request<MyResponse>(new MyRequest(), options);

                    c.CallbackFired = true;
                }))
                .Done(c => c.CallbackFired)
                .Run();

            Assert.True(context.CallbackFired);
        }

        class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
        }

        class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                    c.MakeInstanceUniquelyAddressable("1"));
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Assert.False(Context.CallbackFired);

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