namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;
    using ScenarioDescriptors;

    public class When_a_callback_for_local_message_canceled : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task ShouldNot_trigger_the_callback_when_canceled()
        {
            OperationCanceledException exception = null;

            await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, ctx) =>
                {
                    var cs = new CancellationTokenSource();
                    ctx.TokenSource = cs;

                    var options = new SendOptions();

                    options.RouteToThisEndpoint();

                    try
                    {
                        ctx.Response = await bus.Request<MyResponse>(new MyRequest(), options, cs.Token);
                        ctx.CallbackFired = true;
                    }
                    catch (OperationCanceledException e)
                    {
                        exception = e;
                    }
                }))
                .Done(c => exception != null)
                .Repeat(r => r.For(Transports.Default))
                .Should(c =>
                {
                    Assert.IsNull(c.Response);
                    Assert.False(c.CallbackFired);
                    Assert.True(c.HandlerGotTheRequest);
                    Assert.IsInstanceOf<OperationCanceledException>(exception);
                })
                .Run();
        }

        public class Context : ScenarioContext
        {
            public CancellationTokenSource TokenSource { get; set; }

            public bool HandlerGotTheRequest { get; set; }

            public bool CallbackFired { get; set; }

            public MyResponse Response { get; set; }
        }

        public class EndpointWithLocalCallback : EndpointConfigurationBuilder
        {
            public EndpointWithLocalCallback()
            {
                EndpointSetup<DefaultServer>(c =>
                    c.ScaleOut().InstanceDiscriminator("1"));
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.HandlerGotTheRequest = true;
                    Context.TokenSource.Cancel();

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