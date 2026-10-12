using Azure.Messaging.ServiceBus;

var builder = Host.CreateApplicationBuilder(args);

builder.AddAzureServiceBusClient("servicebus");
builder.Services.AddHostedService<BillingSubscriptionConsumer>();
//builder.Services.AddHostedService<SubscriptionBillingConsumer>();

var host = builder.Build();
host.Run();

class BillingSubscriptionConsumer(ServiceBusClient client, ILogger<BillingSubscriptionConsumer> logger) : BackgroundService
{
    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = client.CreateProcessor("subscription-events", "billing-sub");

        _processor.ProcessMessageAsync += async args =>
        {
            logger.LogInformation("[Billing] Received SubscriptionCancelled event: {Body}", args.Message.Body.ToString());
            await args.CompleteMessageAsync(args.Message);
        };

        _processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "[Billing] Error processing message");
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
            await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}

//class SubscriptionBillingConsumer(ServiceBusClient client, ILogger<SubscriptionBillingConsumer> logger) : BackgroundService
//{
//    private ServiceBusProcessor? _processor;

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        _processor = client.CreateProcessor("subscription-events", "sub-billing-sub");
//        _processor.ProcessMessageAsync += async args =>
//        {
//            logger.LogInformation("[Billing] Received SubscriptionCancelled event: {Body}", args.Message.Body.ToString());
//            await args.CompleteMessageAsync(args.Message);
//        };
//        _processor.ProcessErrorAsync += args =>
//        {
//            logger.LogError(args.Exception, "[Billing] Error processing message");
//            return Task.CompletedTask;
//        };
//        await _processor.StartProcessingAsync(stoppingToken);
//    }

//    public override async Task StopAsync(CancellationToken cancellationToken)
//    {
//        if (_processor is not null) await _processor.StopProcessingAsync(cancellationToken);
//        await base.StopAsync(cancellationToken);
//    }
//}