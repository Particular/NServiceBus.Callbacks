namespace NServiceBus
{
    using System;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.TransportDispatch;

    class SetCallbackResponseReturnCodeBehavior : Behavior<OutgoingContext>
    {
        public override void Invoke(OutgoingContext context, Action next)
        {
            if (context.GetMessageType().IsIntOrEnum())
            {
                var returnCode = context.GetMessageInstance().ConvertToReturnCode();
                context.SetHeader(Headers.ReturnMessageErrorCodeHeader,returnCode);
                context.SetHeader(Headers.ControlMessageHeader, true.ToString());

                context.SkipSerialization();
            }
            next();
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