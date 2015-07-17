﻿namespace NServiceBus
{
    using System;
    using NServiceBus.Features;
    using NServiceBus.OutgoingPipeline;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.TransportDispatch;

    class SetLegacyReturnCodeBehavior : Behavior<OutgoingContext>
    {
        public override void Invoke(OutgoingContext context, Action next)
        {
            if (CallbackSupport.IsLegacyEnumResponse(context.GetMessageType()))
            {
                string returnCode = ((dynamic) context.GetMessageInstance()).ReturnCode;
                context.SetHeader(Headers.ReturnMessageErrorCodeHeader,returnCode);
                context.SetHeader(Headers.ControlMessageHeader, true.ToString());

                context.SkipSerialization();
            }
            next();
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("SetLegacyReturnCode", typeof(SetLegacyReturnCodeBehavior), "Promotes the legacy return code to a header in order to be backwards compatible with v5 and below")
            {

            }
        }
    }
}