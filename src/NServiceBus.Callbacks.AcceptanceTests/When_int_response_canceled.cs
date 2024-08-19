namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;

    public class When_int_response_canceled : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task ShouldNot_trigger_the_callback_when_canceled()
        {
            OperationCanceledException exception = null;

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, c) =>
                {
                    var cs = new CancellationTokenSource();
                    c.TokenSource = cs;

                    var options = new SendOptions();

                    try
                    {
                        c.Response = await bus.Request<int>(new MyRequest(), options, cs.Token);
                        c.CallbackFired = true;
                    }
                    catch (OperationCanceledException ex) when (cs.Token.IsCancellationRequested)
                    {
                        exception = ex;
                    }
                }))
                .WithEndpoint<Replier>()
                .Done(c => exception != null || c.HandlerGotTheRequest)
                .Run();

            Assert.AreNotEqual(200, context.Response);
            Assert.That(context.CallbackFired, Is.False);
            Assert.That(context.HandlerGotTheRequest, Is.True);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }

        public class Context : ScenarioContext
        {
            public CancellationTokenSource TokenSource { get; set; }
            public bool HandlerGotTheRequest { get; set; }
            public bool CallbackFired { get; set; }
            public int Response { get; set; }
        }

        public class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c => c.EnableCallbacks(makesRequests: false));
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

                    return context.Reply(200);
                }
            }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
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