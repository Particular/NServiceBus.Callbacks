namespace NServiceBus.Features
{
    class CallbackSupport : Feature
    {
        public CallbackSupport()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<RequestResponseStateLookup>(DependencyLifecycle.SingleInstance);
            context.Pipeline.Register<RequestResponseInvocationBehavior.Registration>();
            context.Pipeline.Register<UpdateRequestResponseCorrelationTableBehavior.Registration>();
            context.Pipeline.Register<SetCallbackResponseReturnCodeBehavior.Registration>();
            context.Pipeline.Register<SkipBestPracticesForReplyIntEnumBehavior.Registration>();
        }
    }
}