namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.OutgoingPipeline;

    class SetCallbackResponseReturnCodeBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Message.MessageType.IsIntOrEnum())
            {
                var returnCode = context.Message.Instance.ConvertToReturnCode();
                context.Headers[Headers.ReturnMessageErrorCodeHeader] = returnCode;
                context.Headers[Headers.ControlMessageHeader] = true.ToString();

                context.SkipSerialization();
            }
            return next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("SetCallbackResponseReturnCodeBehavior", typeof(SetCallbackResponseReturnCodeBehavior), "Promotes the callback response return code to a header in order to be backwards compatible with v5 and below")
            {
            }
        }
    }
}