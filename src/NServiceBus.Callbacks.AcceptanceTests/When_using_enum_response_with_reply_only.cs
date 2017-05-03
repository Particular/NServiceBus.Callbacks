namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using Features;
    using NUnit.Framework;

    public class When_using_enum_response_with_reply_only : NServiceBusAcceptanceTest
    {
        public enum OldEnum
        {
            Fail,
            Success
        }

        [Test]
        public async Task Should_receive_response_and_not_require_uniquely_addressable()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    c.Response = await bus.Request<OldEnum>(new MyRequest(), new SendOptions());
                    c.CallbackFired = true;
                }))
                .WithEndpoint<Replier>()
                .Done(c => c.CallbackFired)
                .Run();

            Assert.IsNotNull(context.Response);
            Assert.AreEqual(OldEnum.Success, context.Response);
        }

        class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
            public OldEnum Response { get; set; }
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
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(OldEnum.Success);
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