namespace NServiceBus.AcceptanceTests.Callbacks
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Support;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_using_callbacks_in_a_scaleout : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Each_client_should_have_a_unique_input_queue()
        {
            //to avoid processing each others callbacks
            var ctx = await Scenario.Define<Context>(c => { c.Id = Guid.NewGuid(); })
                .WithEndpoint<Client>(b => b.CustomConfig(c => c.MakeInstanceUniquelyAddressable("A"))
                    .When(async (bus, context) =>
                    {
                        var response = await bus.Request<MyResponse>(new MyRequest
                        {
                            Client = "A"
                        }, new SendOptions());
                        context.CallbackAFired = true;
                        if (response.Client != "A")
                        {
                            context.ResponseEndedUpAtTheWrongClient = true;
                        }
                    }))
                .WithEndpoint<Client>(b => b.CustomConfig(c => c.MakeInstanceUniquelyAddressable("B"))
                    .When(async (bus, context) =>
                    {
                        var response = await bus.Request<MyResponse>(new MyRequest
                        {
                            Client = "B"
                        }, new SendOptions());
                        context.CallbackBFired = true;
                        if (response.Client != "B")
                        {
                            context.ResponseEndedUpAtTheWrongClient = true;
                        }
                    }))
                .WithEndpoint<Server>()
                .Done(c => c.CallbackAFired && c.CallbackBFired)
                .Run(new RunSettings());

            Assert.True(ctx.CallbackAFired, "Callback on ClientA should fire");
            Assert.True(ctx.CallbackBFired, "Callback on ClientB should fire");
            Assert.False(ctx.ResponseEndedUpAtTheWrongClient, "One of the responses ended up at the wrong client");
        }

        class Context : ScenarioContext
        {
            public Guid Id { get; set; }

            public bool ResponseEndedUpAtTheWrongClient { get; set; }

            public bool CallbackAFired { get; set; }

            public bool CallbackBFired { get; set; }
        }

        class Client : EndpointConfigurationBuilder
        {
            public Client()
            {
                EndpointSetup<DefaultServer>(c => c.EnableCallbacks())
                    .AddMapping<MyRequest>(typeof(Server));
            }
        }

        class Server : EndpointConfigurationBuilder
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
                    return context.Reply(new MyResponse
                    {
                        Client = message.Client
                    });
                }
            }
        }

        public class MyRequest : IMessage
        {
            public string Client { get; set; }
        }

        public class MyResponse : IMessage
        {
            public string Client { get; set; }
        }
    }
}