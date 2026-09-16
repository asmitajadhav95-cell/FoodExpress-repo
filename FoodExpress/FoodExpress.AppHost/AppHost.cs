var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");
//declares "there is a Service Bus namespace called servicebus,"
//and tells Aspire to run it as a local Docker container instead of a real Azure resource.
var serviceBus = builder.AddAzureServiceBus("servicebus").RunAsEmulator();

//declares the orders queue inside that namespace, as code (this is your infrastructure-as-code for messaging,
var ordersQueue = serviceBus.AddServiceBusQueue("orders");



////This tells Aspire to spin up a local Service Bus emulator 
///(Docker-based — you'll need Docker Desktop running for this one,
///unlike the Redis case we skipped) and inject the connection info into FoodExpress.Server automatically.

var server = builder.AddProject<Projects.FoodExpress_Server>("server") //Server project registration injects the connection info into FoodExpress.Server's configuration automatically
    .WithReference(serviceBus) //
    .WaitFor(serviceBus)  //tells Aspire "don't start server until servicebus is up."
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");



builder.Build().Run();
