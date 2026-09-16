# FoodExpress-repo

Azure event driven application to understand azure services.



Who's who — the terminology, mapped to your actual code

Producer / Publisher: the code inside MapPost("orders", ...) that creates a ServiceBusSender and calls SendMessageAsync. It produces the event.

Event / Message: the OrderPlaced record, serialized to JSON, wrapped in a ServiceBusMessage.

Queue: "orders" — a point-to-point channel. Important nuance: a queue delivers each message to exactly one consumer (if you ran two copies of the consumer, they'd split the work, not both see every message).

Consumer / Subscriber: OrderPlacedConsumer, a BackgroundService using a ServiceBusProcessor. It pulls messages off the queue and acts on them.



Configuration made so far, and what each line does



In FoodExpress.AppHost's AppHost.cs:



builder.AddAzureServiceBus("servicebus").RunAsEmulator() — declares "there is a Service Bus namespace called servicebus," and tells Aspire to run it as a local Docker container instead of a real Azure resource. This line is what would change (drop .RunAsEmulator(), add real Azure config) when you eventually deploy.

serviceBus.AddServiceBusQueue("orders") — declares the orders queue inside that namespace, as code (this is your infrastructure-as-code for messaging, tiny as it is right now).

.WithReference(serviceBus).WaitFor(serviceBus) on the server project registration — injects the connection info into FoodExpress.Server's configuration automatically (no connection string you typed anywhere), and tells Aspire "don't start server until servicebus is up."



In FoodExpress.Server's Program.cs:



builder.AddAzureServiceBusClient("servicebus") — reads that injected connection info and registers a ServiceBusClient in the app's dependency injection container.

builder.Services.AddHostedService<OrderPlacedConsumer>() — registers the consumer as a long-running background task that starts with the app and keeps running for its entire lifetime.

The POST /api/orders endpoint and OrderPlacedConsumer class both get the same ServiceBusClient handed to them via DI — one uses it to create a Sender, the other to create a Processor.

