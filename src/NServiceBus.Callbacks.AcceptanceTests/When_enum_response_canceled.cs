namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_enum_response_canceled : NServiceBusAcceptanceTest
    {
        public enum OldEnum
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
                        c.Response = await bus.Request<OldEnum>(new MyRequest(), options, cs.Token);
                        c.CallbackFired = true;
                    }
                    catch (OperationCanceledException e)
                    {
                        exception = e;
                    }
                }))
                .WithEndpoint<Replier>()
                .Done(c => exception != null || c.HandlerGotTheRequest)
                .Run();

            Assert.AreNotEqual(OldEnum.Success, context.Response);
            Assert.False(context.CallbackFired);
            Assert.True(context.HandlerGotTheRequest);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
        }

        public class Context : ScenarioContext
        {
            public CancellationTokenSource TokenSource { get; set; }
            public bool HandlerGotTheRequest { get; set; }
            public bool CallbackFired { get; set; }
            public OldEnum Response { get; set; }
        }

        public class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c => c.EnableCallbacks(makesRequests: false));
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.HandlerGotTheRequest = true;
                    Context.TokenSource.Cancel();

                    return context.Reply(OldEnum.Success);
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
                        c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(Replier));
                    });
            }
        }

        public class MyRequest : IMessage
        {
        }
    }
}