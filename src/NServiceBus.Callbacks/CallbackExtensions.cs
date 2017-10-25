namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Extensibility;
    using Features;
    using Settings;

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
        /// <param name="enforceUniqueEndpointInstanceAddress">To make sure that replies are routed to the correct endpoint instance each instance must have a unique address.
        /// For some scenarios, like when using a federated transport, this isn't needed. Set this option to `false` to allow non unique instances.</param>
        public static void EnableCallbacks(this EndpointConfiguration config,
            bool makesRequests = true,
            bool enforceUniqueEndpointInstanceAddress = true)
        {
            config.GetSettings().Set(EnforceUniqueEndpointInstanceAddressSettingsKey, enforceUniqueEndpointInstanceAddress);

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

        internal static bool IsUniqueEndpointInstanceAddressRequired(this ReadOnlySettings settings)
        {
            return settings.Get<bool>(EnforceUniqueEndpointInstanceAddressSettingsKey);
        }

        const string EnforceUniqueEndpointInstanceAddressSettingsKey = "Callbacks.EnforceUniqueEndpointInstanceAddress";
    }
}