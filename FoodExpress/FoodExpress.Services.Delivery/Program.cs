using Azure.Messaging.ServiceBus;

var builder = Host.CreateApplicationBuilder(args);

builder.AddAzureServiceBusClient("servicebus");
builder.Services.AddHostedService<DeliverySubscriptionConsumer>();

var host = builder.Build();
host.Run();

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