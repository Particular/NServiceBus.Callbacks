namespace NServiceBus.Features
{
    class CallbackRequestSupport : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            var lookup = new RequestResponseStateLookup();
            context.Pipeline.Register("RequestResponseInvocationForControlMessagesBehavior", new RequestResponseInvocationForControlMessagesBehavior(lookup), "Invokes the callback of a synchronous request/response for control messages");
            context.Pipeline.Register("RequestResponseInvocationForMessagesBehavior", new RequestResponseInvocationForMessagesBehavior(lookup), "Invokes the callback of a synchronous request/response");
            context.Pipeline.Register(new UpdateRequestResponseCorrelationTableBehavior.Registration(lookup));
        }
    }
}