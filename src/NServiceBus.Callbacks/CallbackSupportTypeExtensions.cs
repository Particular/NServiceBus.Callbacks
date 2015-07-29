namespace NServiceBus
{
    using System;

    static class CallbackSupportTypeExtensions
    {
        internal static bool IsCallbackResponse(this Type instanceType)
        {
            return instanceType.IsGenericType
                   && instanceType.GetGenericTypeDefinition() == typeof(CallbackResponse<>);
        }

        internal static bool IsIntOrEnum(this Type instanceType)
        {
            return instanceType.IsEnum || instanceType == typeof(Int32) || instanceType == typeof(Int16) || instanceType == typeof(Int64);
        }
    }
}