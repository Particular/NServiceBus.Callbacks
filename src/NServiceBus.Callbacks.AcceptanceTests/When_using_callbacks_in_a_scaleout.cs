namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NServiceBus.Support;
    using NUnit.Framework;

    public class When_using_callbacks_in_a_scaleout : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Each_client_should_have_a_unique_input_queue()
        {
            //to avoid processing each others callbacks
            await Scenario.Define<Context>(c => { c.Id = Guid.NewGuid(); })
                    .WithEndpoint<Client>(b => b.CustomConfig(c => RuntimeEnvironment.MachineNameAction = () => "ClientA")
                        .When(async (bus, context) =>
                        {
                            await bus.Request<MyResponse>(new MyRequest
                            {
                                Id = context.Id,
                                Client = RuntimeEnvironment.MachineName
                            }, new SendOptions());
                            context.CallbackAFired = true;
                        }))
                    .WithEndpoint<Client>(b => b.CustomConfig(c => RuntimeEnvironment.MachineNameAction = () => "ClientB")
                        .When(async (bus, context) =>
                        {
                            await bus.Request<MyResponse>(new MyRequest
                            {
                                Id = context.Id,
                                Client = RuntimeEnvironment.MachineName
                            }, new SendOptions());
                            context.CallbackBFired = true;
                        }))
                    .WithEndpoint<Server>()
                    .Done(c => c.ClientAGotResponse && c.ClientBGotResponse)
                    .Repeat(r => r.For<AllTransportsWithCentralizedPubSubSupport>())
                    .Should(c =>
                        {
                            Assert.True(c.CallbackAFired, "Callback on ClientA should fire");
                            Assert.True(c.CallbackBFired, "Callback on ClientB should fire");
                            Assert.False(c.ResponseEndedUpAtTheWrongClient, "One of the responses ended up at the wrong client");
                        })
                      .Run(new RunSettings());
        }

        public class Context : ScenarioContext
        {
            public Guid Id { get; set; }

            public bool ClientAGotResponse { get; set; }

            public bool ClientBGotResponse { get; set; }

            public bool ResponseEndedUpAtTheWrongClient { get; set; }

            public bool CallbackAFired { get; set; }

            public bool CallbackBFired { get; set; }
        }

        public class Client : EndpointConfigurationBuilder
        {
            public Client()
            {
                EndpointSetup<DefaultServer>(c => c.UniquelyIdentifyRunningInstance())
                    .AddMapping<MyRequest>(typeof(Server));
            }

            public class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public Context Context { get; set; }


                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    if (Context.Id != message.Id)
                    {
                        return Task.FromResult(0);
                    }

                    if (RuntimeEnvironment.MachineName == "ClientA")
                        Context.ClientAGotResponse = true;
                    else
                    {
                        Context.ClientBGotResponse = true;
                    }

                    if (RuntimeEnvironment.MachineName != message.Client)
                    {
                        Context.ResponseEndedUpAtTheWrongClient = true;
                    }

                    return Task.FromResult(0);
                }
            }
        }

        public class Server : EndpointConfigurationBuilder
        {
            public Server()
            {
                EndpointSetup<DefaultServer>();
            }

            public class MyMessageHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    if (Context.Id != message.Id)
                    {
                        return Task.FromResult(0);
                    }

                    return context.Reply(new MyResponse { Id = message.Id, Client = message.Client });
                }
            }
        }

        public class MyRequest : IMessage
        {
            public Guid Id { get; set; }

            public string Client { get; set; }
        }

        public class MyResponse : IMessage
        {
            public Guid Id { get; set; }

            public string Client { get; set; }
        }
    }
}
