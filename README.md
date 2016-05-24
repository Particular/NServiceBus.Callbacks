# NServiceBus.Callbacks

This library provides extensions for NServiceBus which allow to define a callback on send operations. Callbacks can be used to map a response message to a stateful resource (e.g. a HTTP Request) without the need to use a message handler.


### Getting started

To get started, install the package via NuGet:

```
Install-Package NServiceBus.Callbacks
``` 

For more information please read our documentation about [Handling Responses on the Client Side](http://docs.particular.net/nservicebus/messaging/handling-responses-on-the-client-side) and the [Callbacks sample project](http://docs.particular.net/samples/callbacks/).
