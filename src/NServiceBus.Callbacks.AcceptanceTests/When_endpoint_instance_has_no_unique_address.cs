namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_endpoint_instance_has_no_unique_address : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_allow_requests_routed_to_any_instance()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithNoUniqueAddress>(e => e.When(async (s, c) =>
                {
                    var options = new SendOptions();

                    options.RouteReplyToAnyInstance();
                    options.RouteToThisEndpoint();

                    await s.Request<MyResponse>(new MyRequest(), options);

                    c.GotResponse = true;
                }))
                .Done(c => c.GotResponse)
                .Run();

            Assert.True(context.GotResponse);
        }

        [Test]
        public async Task Should_allow_requests_routed_to_custom_address()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithNoUniqueAddress>(e => e.When(async (s, c) =>
                {
                    var options = new SendOptions();

                    options.RouteReplyTo(AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(typeof(EndpointWithNoUniqueAddress)));
                    options.RouteToThisEndpoint();

                    await s.Request<MyResponse>(new MyRequest(), options);

                    c.GotResponse = true;
                }))
                .Done(c => c.GotResponse)
                .Run();

            Assert.True(context.GotResponse);
        }

        [Test]
        public void Should_throw_reply_is_routed_to_specific_instance()
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