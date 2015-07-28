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
            context.Settings.Get<Conventions>().AddSystemMessagesConventions(CallbackSupportTypeExtensions.IsLegacyEnumResponse);
            context.Container.ConfigureComponent<RequestResponseStateLookup>(DependencyLifecycle.SingleInstance);
            context.Pipeline.Register<RequestResponseInvocationBehavior.Registration>();
            context.Pipeline.Register<UpdateRequestResponseCorrelationTableBehavior.Registration>();
            context.Pipeline.Register<SetLegacyReturnCodeBehavior.Registration>();
        }
    }
}