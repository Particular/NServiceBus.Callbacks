namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NUnit.Framework;

    public class When_controlmessage_response_does_not_match_expected_type : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_fail_the_request_task()
        {
            Exception exception = null;
            Task requestTask = null;

            await Scenario.Define<Context>()
                .WithEndpoint<Replier>()
                .WithEndpoint<EndpointWithLocalCallback>(b => b.When(async (bus, ctx) =>
                {
                    try
                    {
                        requestTask = bus.Request<MyResponse>(new MyRequest());

                        await requestTask;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                }).DoNotFailOnErrorMessages())
                .Done(c => exception != null)
                .Run();

            Assert.Multiple(() =>
            {
                Assert.That(requestTask.Status, Is.EqualTo(TaskStatus.Faulted));
                Assert.That(exception, Is.Not.Null);
            });
            Assert.That(exception.GetType(), Is.EqualTo(typeof(ArgumentException)));
        }

        class Context : ScenarioContext
        {
        }

        class Replier : EndpointConfigurationBuilder
        {
            public Replier()
            {
                EndpointSetup<DefaultServer>(c => c.EnableCallbacks(makesRequests: false));
            }

            public class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(42);
                }
            }
        }

        class EndpointWithLocalCallback : EndpointConfigurationBuilder
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

        public class MyResponse : IMessage
        {
        }
    }
}