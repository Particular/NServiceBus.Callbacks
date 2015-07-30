namespace NServiceBus
{
    using System;
    using System.Linq;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;

    class RequestResponseInvocationBehavior : LogicalMessagesProcessingStageBehavior
    {
        RequestResponseStateLookup requestResponseStateLookup;

        public RequestResponseInvocationBehavior(RequestResponseStateLookup requestResponseStateLookup)
        {
            this.requestResponseStateLookup = requestResponseStateLookup;
        }

        public override void Invoke(Context context, Action next)
        {
            if (HandleCorrelatedMessage(context.GetPhysicalMessage(), context))
            {
                context.MessageHandled = true;
            }

            next();
        }

        bool HandleCorrelatedMessage(TransportMessage transportMessage, Context context)
        {
            var correlationId = context.GetCorrelationId();

            if (correlationId == null)
            {
                return false;
            }

            string version;
            var checkMessageIntent = true;

            if (transportMessage.Headers.TryGetValue(Headers.NServiceBusVersion, out version))
            {
                if (version.StartsWith("3."))
                {
                    checkMessageIntent = false;
                }
            }

            if (checkMessageIntent && transportMessage.MessageIntent != MessageIntentEnum.Reply)
            {
                return false;
            }

            TaskCompletionSourceAdapter tcs;
            if (!requestResponseStateLookup.TryGet(correlationId, out tcs))
            {
                return false;
            }

            object result;

            if (IsControlMessage(context.GetPhysicalMessage()))
            {
                var responseType = tcs.ResponseType;
                var errorCode = transportMessage.Headers[Headers.ReturnMessageErrorCodeHeader];
                result = errorCode.ConvertFromReturnCode(responseType);
            }
            else
            {
                result = context.LogicalMessages.First().Instance;
            }

            tcs.SetResult(result);

            return true;
        }

        static bool IsControlMessage(TransportMessage transportMessage)
        {
            return transportMessage.Headers != null &&
                   transportMessage.Headers.ContainsKey(Headers.ControlMessageHeader);
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("RequestResponseInvocation", typeof(RequestResponseInvocationBehavior), "Invokes the callback of a synchronous request/response")
            {
            }
        }
    }
}