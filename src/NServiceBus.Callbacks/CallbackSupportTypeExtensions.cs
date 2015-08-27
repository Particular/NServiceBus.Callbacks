namespace NServiceBus
{
    using System;

    static class CallbackSupportTypeExtensions
    {
        internal static bool IsIntOrEnum(this Type instanceType)
        {
            return instanceType.IsEnum() ||
                   instanceType.IsInt();
        }

        internal static bool IsEnum(this Type instanceType)
        {
            return instanceType.IsEnum;
        }

        static bool IsInt(this Type instanceType)
        {
            return instanceType == typeof(int) ||
                   instanceType == typeof(short) ||
                   instanceType == typeof(long);
        }
    }
}