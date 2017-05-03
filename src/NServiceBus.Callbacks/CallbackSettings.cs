namespace NServiceBus.Features
{
    /// <summary>
    /// Callback configuration instance.
    /// </summary>
    public class CallbackSettings
    {
        internal CallbackSettings(EndpointConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Enables the possibility to reply to callbacks without requiring to make the endpoint uniquely
        /// addressable.
        /// </summary>
        public void EnableCallbackRepliesOnly()
        {
            config.DisableFeature<CallbackRequestSupport>();
        }

        /// <summary>
        /// Disables the callbacks entirely.
        /// </summary>
        public void DisableCallbacks()
        {
            config.DisableFeature<CallbackRequestSupport>();
            config.DisableFeature<CallbackReplySupport>();
        }

        EndpointConfiguration config;
    }
}