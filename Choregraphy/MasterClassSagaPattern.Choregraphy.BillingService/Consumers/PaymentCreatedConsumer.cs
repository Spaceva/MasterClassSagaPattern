using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class PaymentCreatedConsumer : IConsumer<PaymentCreated>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<PaymentCreatedConsumer> logger;

    public PaymentCreatedConsumer(BillingDbContext dbContext, ILogger<PaymentCreatedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCreated> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var quantity = context.Message.Quantity;
        var amount = context.Message.Amount;

        logger.LogInformation("Received payment with { Id '{id}', Quantity = {quantity}, Amount = {amount}€ }. Processing...", id, quantity, amount);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var billing = new Billing { Id = id };

        dbContext.Add(billing);

        await dbContext.SaveChangesAsync();

        logger.LogInformation($"Processed.");

        await context.Publish<BillingCompleted>(new { context.CorrelationId });
    }
}
