namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_using_enum_response_and_conventions : NServiceBusAcceptanceTest
    {
        public enum OldEnum
        {
            Fail,
            Success
        }

        [Test]
        public async Task Should_send_back_old_style_control_message()
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

        public class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
            public OldEnum Response { get; set; }
        }

        public class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(busConfiguration =>
                {
                    var conventions = busConfiguration.Conventions();
                    conventions.DefiningCommandsAs(DefinesCommandType);
                    busConfiguration.ScaleOut().InstanceDiscriminator("1");
                });
            }

            bool DefinesCommandType(Type type)
            {
                return typeof(OldEnum) == type;
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(OldEnum.Success);
                }
            }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                    c.ScaleOut().InstanceDiscriminator("1"))
                    .AddMapping<MyRequest>(typeof(Replier));
            }
        }

        public class MyRequest : IMessage
        {
        }
    }
}