namespace NServiceBus
{
    using Features;

    /// <summary>
    /// Extension methods to configure callbacks on <see cref="EndpointConfiguration" />.
    /// </summary>
    public static class CallbackExtensions
    {
        /// <summary>
        /// Enables the callbacks feature.
        /// </summary>
        /// <param name="config">The endpoint configuration.</param>
        /// <param name="makesRequests">
        /// The value that indicates whether the endpoint can make callback requests. If <c>true</c>, the endpoint must be uniquely
        /// addressable.
        /// </param>
        public static void EnableCallbacks(this EndpointConfiguration config, bool makesRequests = true)
        {
            if (makesRequests)
            {
                config.EnableFeature<CallbackRequestSupport>();
                config.EnableFeature<CallbackReplySupport>();
            }
            else
            {
                config.EnableFeature<CallbackReplySupport>();
            }
        }
    }
}