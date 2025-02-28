﻿namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
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

            Assert.That(context.CallbackFired, Is.True);
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
                {
                    c.MakeInstanceUniquelyAddressable("1");
                    c.EnableCallbacks();
                });
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                Context testContext;

                public MyRequestHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Assert.That(testContext.CallbackFired, Is.False);

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