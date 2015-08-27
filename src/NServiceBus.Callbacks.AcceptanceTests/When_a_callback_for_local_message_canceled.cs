namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_a_callback_for_local_message_canceled : NServiceBusAcceptanceTest
    {
        [Test]
        public void ShouldNot_trigger_the_callback_when_canceled()
        {
            OperationCanceledException exception = null;
            var context = new Context();

            Scenario.Define(context)
                    .WithEndpoint<EndpointWithLocalCallback>(b => b.Given(async (bus, ctx) =>
                        {
                            var cs = new CancellationTokenSource();
                            ctx.TokenSource = cs;

                            var options = new SendOptions();

                            options.RouteToLocalEndpointInstance();
                            options.RegisterCancellationToken(cs.Token);

                            var response = bus.Request<MyResponse>(new MyRequest(), options);

                            try
                            {
                                ctx.Response = await response;
                                ctx.CallbackFired = true;
                            }
                            catch (OperationCanceledException e)
                            {
                                exception = e;
                            }
                        }))
                    .Done(c => exception != null)
                    .Repeat(r => r.For(Transports.Default))
                    .Run();

            Assert.IsNull(context.Response);
            Assert.False(context.CallbackFired);
            Assert.True(context.HandlerGotTheRequest);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
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
                EndpointSetup<DefaultServer>();
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public IBus Bus { get; set; }

                public void Handle(MyRequest request)
                {
                    Context.HandlerGotTheRequest = true;
                    Context.TokenSource.Cancel();

                    Bus.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage { }

        public class MyResponse : IMessage { }
    }
}