namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using Configuration.AdvancedExtensibility;

    public static class EndpointConfigurationExtensions
    {
        public static TransportExtensions ConfigureTransport(this EndpointConfiguration endpointConfiguration)
        {
            return new TransportExtensions(endpointConfiguration.GetSettings());
        }

        public static void RouteToEndpoint(this RoutingSettings routingSettings, Type messageType, Type destinationEndpointType)
        {
            var destinationEndpointAddress = AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(destinationEndpointType);
            routingSettings.RouteToEndpoint(messageType, destinationEndpointAddress);
        }
    }
}