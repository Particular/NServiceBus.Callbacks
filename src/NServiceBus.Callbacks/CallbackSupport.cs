namespace NServiceBus.Features
{
    using System;

    class CallbackSupport : Feature
    {
        public CallbackSupport()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            if (!context.Settings.HasSetting("EndpointInstanceDiscriminator"))
            {
                throw new Exception("In order to use the callbacks feature you need to specify an endpoint instance ID via EndpointConfiguration.ScaleOut().InstanceDiscriminator(string discriminator)");
            }

            context.Container.ConfigureComponent<RequestResponseStateLookup>(DependencyLifecycle.SingleInstance);
            context.Pipeline.Register<RequestResponseInvocationForControlMessagesBehavior.Registration>();
            context.Pipeline.Register<RequestResponseInvocationForMessagesBehavior.Registration>();
            context.Pipeline.Register<UpdateRequestResponseCorrelationTableBehavior.Registration>();
            context.Pipeline.Register<SetCallbackResponseReturnCodeBehavior.Registration>();
            context.Pipeline.Register<SkipBestPracticesForReplyIntEnumBehavior.Registration>();
        }
    }
}