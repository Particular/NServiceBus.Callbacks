namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTesting.EndpointTemplates;
    using NServiceBus.AcceptanceTesting.Customization;

    public class When_using_enum_response_and_conventions : NServiceBusAcceptanceTest
    {
        public enum ResponseStatus
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
                    c.Response = await bus.Request<ResponseStatus>(new MyRequest(), new SendOptions());
                    c.CallbackFired = true;
                }))
                .WithEndpoint<Replier>()
                .Done(c => c.CallbackFired)
                .Run();

            Assert.IsNotNull(context.Response);
            Assert.AreEqual(ResponseStatus.Success, context.Response);
        }

        class Context : ScenarioContext
        {
            public bool CallbackFired { get; set; }
            public ResponseStatus Response { get; set; }
        }

        class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var conventions = c.Conventions();
                    conventions.DefiningCommandsAs(DefinesCommandType);
                    c.EnableCallbacks(makesRequests: false);
                });
            }

            bool DefinesCommandType(Type type)
            {
                return typeof(ResponseStatus) == type;
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(ResponseStatus.Success);
                }
            }
        }

        class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.MakeInstanceUniquelyAddressable("1");
                    c.EnableCallbacks();
                    c.ConfigureRouting().RouteToEndpoint(typeof(MyRequest), typeof(Replier));
                });
            }
        }

        public class MyRequest : IMessage
        {
        }
    }
}