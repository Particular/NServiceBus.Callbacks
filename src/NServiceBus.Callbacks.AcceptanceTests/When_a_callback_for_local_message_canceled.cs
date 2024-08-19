namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;

    public class When_a_callback_for_local_message_canceled : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task ShouldNot_trigger_the_callback_when_canceled()
        {
            OperationCanceledException exception = null;

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, ctx) =>
                {
                    var cs = new CancellationTokenSource();
                    ctx.TokenSource = cs;

                    var options = new SendOptions();

                    options.RouteToThisEndpoint();

                    try
                    {
                        ctx.ResponseViaCallback = await bus.Request<MyResponse>(new MyRequest(), options, cs.Token);
                        ctx.CallbackFired = true;
                    }
                    catch (OperationCanceledException ex) when (cs.Token.IsCancellationRequested)
                    {
                        exception = ex;
                    }
                }))
                .Done(c => exception != null && c.GotTheResponseMessage)
                .Run();

            Assert.Multiple(() =>
            {
                Assert.That(context.GotTheResponseMessage, Is.True);
                Assert.That(context.ResponseViaCallback, Is.Null);
                Assert.That(context.CallbackFired, Is.False);
                Assert.That(context.HandlerGotTheRequest, Is.True);
                Assert.That(exception, Is.InstanceOf<OperationCanceledException>());
            });
        }

        public class Context : ScenarioContext
        {
            public CancellationTokenSource TokenSource { get; set; }

            public bool HandlerGotTheRequest { get; set; }

            public bool CallbackFired { get; set; }

            public MyResponse ResponseViaCallback { get; set; }

            public bool GotTheResponseMessage { get; set; }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
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
                    testContext.HandlerGotTheRequest = true;
                    testContext.TokenSource.Cancel();

                    return context.Reply(new MyResponse());
                }
            }

            public class MyResponseHandler : IHandleMessages<MyResponse>
            {
                Context testContext;

                public MyResponseHandler(Context testContext)
                {
                    this.testContext = testContext;
                }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    testContext.GotTheResponseMessage = true;

                    return Task.FromResult(0);
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