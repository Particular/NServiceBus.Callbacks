namespace NServiceBus
{
    using System;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Pipeline;

    class SkipBestPracticesForReplyIntEnumBehavior : Behavior<OutgoingReplyContext>
    {
        public override void Invoke(OutgoingReplyContext context, Action next)
        {
            if (context.GetMessageType().IsIntOrEnum())
            {
                context.DoNotEnforceBestPractices();
            }
            next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base(
                    "SkipBestPracticesForReplyIntEnumBehavior",
                    typeof(SkipBestPracticesForReplyIntEnumBehavior),
                    "Skip BestPractices message validation for int and enum Reply")
            {
                InsertBefore(WellKnownStep.EnforceReplyBestPractices);
            }
        }
    }
}