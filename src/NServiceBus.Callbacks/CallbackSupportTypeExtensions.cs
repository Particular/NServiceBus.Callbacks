namespace NServiceBus
{
    using System;

    static class CallbackSupportTypeExtensions
    {
        internal static bool IsLegacyEnumResponse(this Type instanceType)
        {
            return instanceType.IsGenericType
                   && instanceType.GetGenericTypeDefinition() == typeof(LegacyEnumResponse<>);
        }
    }
}