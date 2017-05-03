namespace NServiceBus.Features
{
    /// <summary>
    /// Extension methods to configure callbacks on <see cref="EndpointConfiguration"/>.
    /// </summary>
    public static class CallbackExtensions
    {
        /// <summary>
        /// Allows to configure callbacks.
        /// </summary>
        public static CallbackSettings Callbacks(this EndpointConfiguration config)
        {
            return new CallbackSettings(config);
        }
    }
}