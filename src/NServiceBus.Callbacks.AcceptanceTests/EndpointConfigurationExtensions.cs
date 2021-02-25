namespace NServiceBus.Callbacks.AcceptanceTests
{
    using System;
    using Configuration.AdvancedExtensibility;

    public static class EndpointConfigurationExtensions
    {
        public static RoutingSettings ConfigureRouting(this EndpointConfiguration configuration) =>
             new RoutingSettings(configuration.GetSettings());

        public static void RouteToEndpoint(this RoutingSettings routingSettings, Type messageType, Type destinationEndpointType)
        {
            var destinationEndpointAddress = AcceptanceTesting.Customization.Conventions.EndpointNamingConvention(destinationEndpointType);
            routingSettings.RouteToEndpoint(messageType, destinationEndpointAddress);
        }
    }
}