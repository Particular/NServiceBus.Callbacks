namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

    class SkipBestPracticesForReplyIntEnumBehavior : Behavior<IOutgoingReplyContext>
    {
        public override Task Invoke(IOutgoingReplyContext context, Func<Task> next)
        {
            if (context.Message.MessageType.IsIntOrEnum())
            {
                context.DoNotEnforceBestPractices();
            }
            return next();
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