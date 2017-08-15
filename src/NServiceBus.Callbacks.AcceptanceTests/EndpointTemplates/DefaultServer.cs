namespace NServiceBus.AcceptanceTests.EndpointTemplates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AcceptanceTesting.Customization;
    using AcceptanceTesting.Support;

    public class DefaultServer : IEndpointSetupTemplate
    {
        public DefaultServer()
        {
            typesToInclude = new List<Type>();
        }

        public DefaultServer(List<Type> typesToInclude)
        {
            this.typesToInclude = typesToInclude;
        }

        public Task<EndpointConfiguration> GetConfiguration(RunDescriptor runDescriptor, EndpointCustomizationConfiguration endpointConfiguration, Action<EndpointConfiguration> configurationBuilderCustomization)
        {
            var types = endpointConfiguration.GetTypesScopedByTestClass();

            typesToInclude.AddRange(types);

            var configuration = new EndpointConfiguration(endpointConfiguration.EndpointName);

            configuration.TypesToIncludeInScan(typesToInclude);

            var recoverability = configuration.Recoverability();
            recoverability.Delayed(delayed => delayed.NumberOfRetries(0));
            recoverability.Immediate(immediate => immediate.NumberOfRetries(0));

            var storageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".acpttests"); //, NUnit.Framework.TestContext.CurrentContext.Test.ID);

            configuration.UseTransport<LearningTransport>()
                .StorageDirectory(storageDir);

            configuration.RegisterComponentsAndInheritanceHierarchy(runDescriptor);

            configurationBuilderCustomization(configuration);

            //runDescriptor.OnTestCompleted(rst =>
            //{
            //    //if (Directory.Exists(storageDir))
            //    //{
            //    //    Directory.Delete(storageDir, true);
            //    //}

            //    return Task.FromResult(0);
            //});

            return Task.FromResult(configuration);
        }

        List<Type> typesToInclude;
    }
}