namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_routing_reply_to_specific_non_unique_instance : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_throw_if_requested_explicitly()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithNoUniqueAddress>(e => e.When(async (s, c) =>
                {
                    var options = new SendOptions();

                    options.RouteReplyToThisInstance();
                    options.RouteToThisEndpoint();

                    await s.Request<MyResponse>(new MyRequest(), options);
                }))
                .Done(c => c.EndpointsStarted)
                .Run());

            StringAssert.Contains("MakeInstanceUniquelyAddressable", ex.Message);
        }

        [Test]
        public void Should_throw_by_default()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithNoUniqueAddress>(e => e.When(async (s, c) =>
                {
                    var options = new SendOptions();

                   options.RouteToThisEndpoint();

                    await s.Request<MyResponse>(new MyRequest(), options);
                }))
                .Done(c => c.EndpointsStarted)
                .Run());

            StringAssert.Contains("MakeInstanceUniquelyAddressable", ex.Message);
        }

        public class Context : ScenarioContext
        {
            public bool GotResponse { get; set; }
        }

        public class EndpointWithNoUniqueAddress : EndpointConfigurationBuilder
        {
            public EndpointWithNoUniqueAddress()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.EnableCallbacks();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
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