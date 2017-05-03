namespace NServiceBus.Features
{
    class CallbackReplySupport : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register<SkipBestPracticesForReplyIntEnumBehavior.Registration>();
            context.Pipeline.Register("SetCallbackResponseReturnCodeBehavior", new SetCallbackResponseReturnCodeBehavior(), "Promotes the callback response return code to a header in order to be backwards compatible with v5 and below");
        }
    }
}