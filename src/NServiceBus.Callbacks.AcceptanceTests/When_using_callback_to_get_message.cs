namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_using_callback_to_get_message : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_message()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    c.Response = await bus.Request<MyResponse>(new MyRequest(), new SendOptions());
                    c.CallbackFired = true;
                }))
                .WithEndpoint<Replier>()
                .Done(c => c.CallbackFired)
                .Run();

            Assert.IsNotNull(context.Response);
        }

        class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
            public MyResponse Response { get; set; }
        }

        class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c =>
                    c.MakeInstanceUniquelyAddressable("1"));
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(new MyResponse());
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

        public class MyResponse : IMessage
        {
        }
    }
}