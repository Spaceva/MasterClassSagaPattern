using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.BillingService;

public class PrepareBillingConsumer : IConsumer<PrepareBilling>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<PrepareBillingConsumer> logger;

    public PrepareBillingConsumer(BillingDbContext dbContext, ILogger<PrepareBillingConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PrepareBilling> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var address = context.Message.Address;
        var quantity = context.Message.Quantity;
        var amount = context.Message.Amount;

        logger.LogInformation("Received {command} message with { Id = '{id}', Address = '{address}', Quantity = {quantity}, Amount = {amount} }.", nameof(PrepareBilling), id, address, quantity, amount);

        dbContext.Add(new Billing { Id = id, Address = address, Quantity = quantity, Amount = amount });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Delivery '{id}' created. Waiting for Payment and Stock services.", id);

        await context.Publish<BillingPrepared>(new { context.CorrelationId });
    }
}
