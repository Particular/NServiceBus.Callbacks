namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_enum_response_canceled : NServiceBusAcceptanceTest
    {
        [Test]
        public void ShouldNot_trigger_the_callback_when_canceled()
        {
            OperationCanceledException exception = null;
            var context = new Context();

            Scenario.Define(context)
                .WithEndpoint<EndpointWithLocalCallback>(b => b.Given(async (bus, c) =>
                {
                    var cs = new CancellationTokenSource();
                    context.TokenSource = cs;

                    var options = new SendOptions();

                    options.RegisterCancellationToken(cs.Token);

                    var response = bus.Request<OldEnum>(new MyRequest(), options);
                    try
                    {
                        c.Response = await response;
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

        [Serializable]
        public class MyRequest : IMessage { }

        public enum OldEnum
        {
            Fail,
            Success,
        }
    }
}