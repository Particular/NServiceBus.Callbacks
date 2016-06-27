namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Transports;

    class RequestResponseInvocationForControlMessagesBehavior : Behavior<IIncomingPhysicalMessageContext>
    {
        public RequestResponseInvocationForControlMessagesBehavior(RequestResponseStateLookup requestResponseStateLookup)
        {
            this.requestResponseStateLookup = requestResponseStateLookup;
        }

        public override Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            AssignResultIfPossible(context.Extensions.Get<IncomingMessage>(), context);

            return next();
        }

        void AssignResultIfPossible(IncomingMessage incomingMessage, IIncomingContext context)
        {
            if (!IsControlMessage(incomingMessage))
            {
                return;
            }

            var result = context.TryRemoveResponseStateBasedOnCorrelationId(incomingMessage, requestResponseStateLookup);
            if (!result.HasValue)
            {
                return;
            }

            var responseType = result.Value.TaskCompletionSource.ResponseType;
            var errorCode = incomingMessage.Headers[Headers.ReturnMessageErrorCodeHeader];
            result.Value.TaskCompletionSource.TrySetResult(errorCode.ConvertFromReturnCode(responseType));
        }

        static bool IsControlMessage(IncomingMessage incomingMessage)
        {
            return incomingMessage.Headers != null &&
                   incomingMessage.Headers.ContainsKey(Headers.ControlMessageHeader);
        }

        RequestResponseStateLookup requestResponseStateLookup;

        public class Registration : RegisterStep
        {
            public Registration()
                : base("RequestResponseInvocationForControlMessagesBehavior", typeof(RequestResponseInvocationForControlMessagesBehavior), "Invokes the callback of a synchronous request/response for control messages")
            {
            }
        }
    }
}