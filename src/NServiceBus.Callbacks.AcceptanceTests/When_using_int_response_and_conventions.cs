namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTesting.EndpointTemplates;
    using NServiceBus.AcceptanceTesting.Customization;

    public class When_using_int_response_and_conventions : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_send_back_old_style_control_message()
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
                {
                    var conventions = c.Conventions();
                    conventions.DefiningCommandsAs(DefinesCommandType);
                    c.EnableCallbacks(makesRequests: false);
                });
            }

            bool DefinesCommandType(Type type)
            {
                return typeof(int) == type;
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
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