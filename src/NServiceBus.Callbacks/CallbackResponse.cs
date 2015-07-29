namespace NServiceBus
{
    using System;

    /// <summary>
    /// Message response wrapper used for compatibility purposes with previous versions of the core callback.
    /// </summary>
    public class CallbackResponse<T>
    {
        // ReSharper disable once NotAccessedField.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // This member is used by <see cref="SetLegacyReturnCodeBehavior">
        internal string ReturnCode;

        /// <summary>
        /// Creates an instance of <see cref="CallbackResponse{T}"/>.
        /// </summary>
        /// <param name="status">The enum to set.</param>
        public CallbackResponse(T status)
        {
            Status = status;
            var tType = status.GetType();
            if (!tType.IsIntOrEnum())
            {
                throw new ArgumentException("The status can only be an enum or an integer.", "status");
            }

            ReturnCode = status.ToString();
            if (tType.IsEnum)
            {
                ReturnCode = Enum.Format(tType, status, "D");
            }
        }

        /// <summary>
        /// Contains the status value.
        /// </summary>
        public T Status { get; private set; }
    }
}