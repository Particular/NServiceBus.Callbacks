# NServiceBus.Callbacks

NServiceBus.Callbacks is an extension for NServiceBus to support defining callbacks on send operations. Callbacks can be used to map a response message to a stateful resource (e.g. an HTTP Request) without needing to use a message handler.

It is part of the [Particular Service Platform](https://particular.net/service-platform), which includes [NServiceBus](https://particular.net/nservicebus) and tools to build, monitor, and debug distributed systems.

## Official documentation

See the [Client-side callbacks documentation](https://docs.particular.net/nservicebus/messaging/callbacks) for more details on how to use it.

## Running tests locally

To test callbacks, install the testing package via NuGet:

```
Install-Package NServiceBus.Callbacks.Testing
```

## Contributing

If you are interested in contributing, please follow the instructions on [How to contribute](https://docs.particular.net/platform/contributing).
