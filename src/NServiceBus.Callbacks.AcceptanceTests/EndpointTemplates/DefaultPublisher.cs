namespace NServiceBus.AcceptanceTests.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.Config.ConfigurationSource;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Routing.StorageDrivenPublishing;

    public class DefaultPublisher : IEndpointSetupTemplate
    {
        public BusConfiguration GetConfiguration(RunDescriptor runDescriptor, EndpointConfiguration endpointConfiguration, IConfigurationSource configSource, Action<BusConfiguration> configurationBuilderCustomization)
        {
            return new DefaultServer(new List<Type> { typeof(SubscriptionTracer), typeof(SubscriptionTracer.Registration) }).GetConfiguration(runDescriptor, endpointConfiguration, configSource, b =>
            {
                b.Pipeline.Register<SubscriptionTracer.Registration>();
                configurationBuilderCustomization(b);
            });
        }

        class SubscriptionTracer : Behavior<OutgoingContext>
        {
            public ScenarioContext Context { get; set; }

            public override void Invoke(OutgoingContext context, Action next)
            {
                next();

                SubscribersForEvent subscribersForEvent;

                if (context.TryGet(out  subscribersForEvent))
                {
                    Context.AddTrace($"Subscribers for {subscribersForEvent.EventType.Name} : {string.Join(";", subscribersForEvent)}");

                    if (!subscribersForEvent.Subscribers.Any())
                    {
                        Context.AddTrace($"No Subscribers found for message {subscribersForEvent.EventType.Name}");
                    }
                }
            }

            public class Registration : RegisterStep
            {
                public Registration()
                    : base("SubscriptionTracer", typeof(SubscriptionTracer), "Traces the list of found subscribers")
                {
                }
            }
        }
    }
}