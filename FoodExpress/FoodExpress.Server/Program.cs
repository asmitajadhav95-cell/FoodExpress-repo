using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddAzureServiceBusClient("servicebus"); // reads the injected connection info and 
//builder.AddRedisClientBuilder("cache")
//.WithOutputCache();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//builder.Services.AddHostedService<DeliverySubscriptionConsumer>();
//builder.Services.AddHostedService<NotificationSubscriptionConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseOutputCache();

//string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("restaurants", () =>
{
    return new[]
    {
        new Restaurant(1, "Spiced Villa", "North Indian", 4.5),
        new Restaurant(2, "Tasty&Bites", "Italian", 4.2),
        new Restaurant(3, "DelhiBelly", "Punjab", 4.7),
        new Restaurant(4, "Kimchi", "Chinese", 4.3),
        new Restaurant(5, "Gourmet", "Mediterranean", 4.6),
        new Restaurant(6, "The Spice Route", "Indian", 4.4),
        new Restaurant(7, "Pasta Paradise", "Italian", 4.1),
        new Restaurant(8, "Taco Town", "Mexican", 4.8),
        new Restaurant(9, "Wok & Roll", "Chinese", 4.0),
        new Restaurant(10, "M Magic", "Mediterranean", 4.9),
        new Restaurant(11, "Curry Corner", "Indian", 4.2),
        new Restaurant(12, "Pizza Palace", "Italian", 4.5),
    };
}).WithName("GetRestaurants");


//Producer/Publisher
api.MapPost("orders", async (OrderRequest request, ServiceBusClient serviceBusClient) =>
{
    var order = new Order(Guid.NewGuid(), request.RestaurantId, request.Items, "Placed");

    var sender = serviceBusClient.CreateSender("orders");
    var evt = new OrderPlaced(order.Id, order.RestaurantId, order.Items);
    var message = new ServiceBusMessage(JsonSerializer.Serialize(evt)) { Subject = "OrderPlaced" };
    await sender.SendMessageAsync(message);

    return Results.Created($"/api/orders/{order.Id}", order);
})
.WithName("PlaceOrder");

api.MapPost("orders/{id}/cancel", async (Guid id, ServiceBusClient serviceBusClient) =>
{
    var sender = serviceBusClient.CreateSender("order-events");
    var evt = new OrderCancelled(id);
    var message = new ServiceBusMessage(JsonSerializer.Serialize(evt)) { Subject = "OrderCancelled" };
    await sender.SendMessageAsync(message);

    return Results.Ok(new { message = "Order cancellation event published", orderId = id });
})
.WithName("CancelOrder");


app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record Restaurant(int id, string name, string cuisine, double rating);

record OrderRequest(int RestaurantId, List<string> Items);
record Order(Guid Id, int RestaurantId, List<string> Items, string Status);
record OrderPlaced(Guid OrderId, int RestaurantId, List<string> Items);


class OrderPlacedConsumer(ServiceBusClient client, ILogger<OrderPlacedConsumer> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = client.CreateProcessor("orders");
        _processor.ProcessMessageAsync += async args =>
        {
            logger.LogInformation("Received OrderPlaced event: {Body}", args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        };
        _processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Error processing Service Bus message");
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}


record OrderCancelled(Guid OrderId);

class DeliverySubscriptionConsumer(ServiceBusClient client, ILogger<DeliverySubscriptionConsumer> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = client.CreateProcessor("order-events", "delivery-sub");
        _processor.ProcessMessageAsync += async args =>
        {
            logger.LogInformation("[Delivery] Received OrderCancelled event: {Body}", args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        };
        _processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "[Delivery] Error processing message");
            return Task.CompletedTask;
        };
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null) await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}

class NotificationSubscriptionConsumer(ServiceBusClient client, ILogger<NotificationSubscriptionConsumer> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = client.CreateProcessor("order-events", "notification-sub");
        _processor.ProcessMessageAsync += async args =>
        {
            logger.LogInformation("[Notification] Received OrderCancelled event: {Body}", args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        };
        _processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "[Notification] Error processing message");
            return Task.CompletedTask;
        };
        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null) await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
namespace YourApp.Extensions
{
    public static class AzureServiceBusExtensions
    {
        // configKey is the configuration key that holds the Service Bus connection string
        public static WebApplicationBuilder AddAzureServiceBusClient(this WebApplicationBuilder builder, string configKey)
        {
            var configuration = builder.Configuration;
            var connectionString = configuration[configKey];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Configuration value '{configKey}' for Service Bus connection string is missing.");
            }

            // Register ServiceBusClient as singleton
            builder.Services.AddSingleton(new ServiceBusClient(connectionString));

            return builder;
        }
    }
}

