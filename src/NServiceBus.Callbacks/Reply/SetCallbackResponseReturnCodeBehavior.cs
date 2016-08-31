namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;

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
    }
}