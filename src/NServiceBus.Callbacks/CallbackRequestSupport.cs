namespace NServiceBus.Features
{
    using System;

    class CallbackRequestSupport : Feature
    {
        public CallbackRequestSupport()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            if (!context.Settings.HasSetting("EndpointInstanceDiscriminator"))
            {
                throw new Exception("In order to use the callbacks feature you need to specify an endpoint instance ID via EndpointConfiguration.MakeInstanceUniquelyAddressable(string discriminator)");
            }

            var lookup = new RequestResponseStateLookup();
            context.Pipeline.Register("RequestResponseInvocationForControlMessagesBehavior", new RequestResponseInvocationForControlMessagesBehavior(lookup), "Invokes the callback of a synchronous request/response for control messages");
            context.Pipeline.Register("RequestResponseInvocationForMessagesBehavior", new RequestResponseInvocationForMessagesBehavior(lookup), "Invokes the callback of a synchronous request/response");
            context.Pipeline.Register(new UpdateRequestResponseCorrelationTableBehavior.Registration(lookup));
        }
    }
}