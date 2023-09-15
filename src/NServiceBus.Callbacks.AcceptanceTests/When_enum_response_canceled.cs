namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTesting.EndpointTemplates;
    using NServiceBus.AcceptanceTesting.Customization;

    public class When_enum_response_canceled : NServiceBusAcceptanceTest
    {
        public enum ResponseStatus
        {
            Fail,
            Success
        }

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
                        c.Response = await bus.Request<ResponseStatus>(new MyRequest(), options, cs.Token);
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

            Assert.AreNotEqual(ResponseStatus.Success, context.Response);
            Assert.False(context.CallbackFired);
            Assert.True(context.HandlerGotTheRequest);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }

        public class Context : ScenarioContext
        {
            public CancellationTokenSource TokenSource { get; set; }
            public bool HandlerGotTheRequest { get; set; }
            public bool CallbackFired { get; set; }
            public ResponseStatus Response { get; set; }
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

                    return context.Reply(ResponseStatus.Success);
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