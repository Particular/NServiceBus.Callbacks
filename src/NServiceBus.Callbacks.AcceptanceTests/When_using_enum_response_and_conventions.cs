namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_enum_response_and_conventions : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_send_back_old_style_control_message()
        {
            var context = new Context();

            Scenario.Define(context)
                .WithEndpoint<EndpointWithLocalCallback>(b => b.Given(async (bus, c) =>
                {
                    var response = bus.Request<OldEnum>(new MyRequest(), new SendOptions());
                    c.Response = await response;
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
                });
            }

            bool DefinesCommandType(Type type)
            {
                return typeof(OldEnum) == type;
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public IBus Bus { get; set; }

                public void Handle(MyRequest request)
                {
                    Bus.Reply(OldEnum.Success);
                }
            }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>()
                    .AddMapping<MyRequest>(typeof(Replier));
            }
        }

        public class MyRequest : IMessage { }

        public enum OldEnum
        {
            Fail,
            Success,
        }
    }
}