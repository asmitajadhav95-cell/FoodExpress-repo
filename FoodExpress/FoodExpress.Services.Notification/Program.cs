using Azure.Messaging.ServiceBus;

var builder = Host.CreateApplicationBuilder(args);

builder.AddAzureServiceBusClient("servicebus");
builder.Services.AddHostedService<NotificationSubscriptionConsumer>();

var host = builder.Build();
host.Run();

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