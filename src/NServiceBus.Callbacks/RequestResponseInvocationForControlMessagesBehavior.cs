namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

    class RequestResponseInvocationForControlMessagesBehavior : Behavior<IncomingPhysicalMessageContext>
    {
        RequestResponseStateLookup requestResponseStateLookup;

        public RequestResponseInvocationForControlMessagesBehavior(RequestResponseStateLookup requestResponseStateLookup)
        {
            this.requestResponseStateLookup = requestResponseStateLookup;
        }

        public override Task Invoke(IncomingPhysicalMessageContext context, Func<Task> next)
        {
            AssignResultIfPossible(context.Extensions.Get<IncomingMessage>(), context);

            return next();
        }

        void AssignResultIfPossible(IncomingMessage incomingMessage, IncomingPhysicalMessageContext context)
        {
            var correlationId = context.GetCorrelationId();

            if (correlationId == null)
            {
                return;
            }

            string version;
            var checkMessageIntent = true;

            if (incomingMessage.Headers.TryGetValue(Headers.NServiceBusVersion, out version))
            {
                if (version.StartsWith("3."))
                {
                    checkMessageIntent = false;
                }
            }

            if (checkMessageIntent && incomingMessage.GetMesssageIntent() != MessageIntentEnum.Reply)
            {
                return;
            }

            TaskCompletionSourceAdapter tcs;
            if (!requestResponseStateLookup.TryGet(correlationId, out tcs))
            {
                return;
            }

            if (IsControlMessage(incomingMessage))
            {
                var responseType = tcs.ResponseType;
                var errorCode = incomingMessage.Headers[Headers.ReturnMessageErrorCodeHeader];
                tcs.SetResult(errorCode.ConvertFromReturnCode(responseType));
                requestResponseStateLookup.RemoveState(correlationId);
            }
        }

        static bool IsControlMessage(IncomingMessage incomingMessage)
        {
            return incomingMessage.Headers != null &&
                   incomingMessage.Headers.ContainsKey(Headers.ControlMessageHeader);
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base("RequestResponseInvocationForControlMessagesBehavior", typeof(RequestResponseInvocationForControlMessagesBehavior), "Invokes the callback of a synchronous request/response for control messages")
            {
            }
        }
    }
}