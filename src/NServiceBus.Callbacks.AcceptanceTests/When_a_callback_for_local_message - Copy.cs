namespace NServiceBus.AcceptanceTests.Callbacks
{
    using AcceptanceTesting;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_opting_out_from_unique_address : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_not_require_unique_instance_address()
        {
            Assert.DoesNotThrowAsync(async () => await Scenario.Define<Context>()
                .WithEndpoint<EndpointWithOptOut>()
                .Done(c => c.EndpointsStarted)
                .Run());
        }

        public class Context : ScenarioContext
        {
        }

        public class EndpointWithOptOut : EndpointConfigurationBuilder
        {
            public EndpointWithOptOut()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.EnableCallbacks();
                });
            }
        }
    }
}