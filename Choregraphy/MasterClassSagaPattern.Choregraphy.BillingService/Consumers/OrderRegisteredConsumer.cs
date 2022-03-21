using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class OrderRegisteredConsumer : IConsumer<OrderRegistered>
{
    private readonly BillingDbContext dbContext;
    private readonly ILogger<OrderRegisteredConsumer> logger;

    public OrderRegisteredConsumer(BillingDbContext dbContext, ILogger<OrderRegisteredConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderRegistered> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var address = context.Message.Address;
        var quantity = context.Message.Quantity;
        var amount = quantity * 2.5f;

        logger.LogInformation("Received {event} message with { Id = '{id}', Address = '{address}', Quantity = {quantity} }.", nameof(OrderRegistered), id, address, quantity);

        dbContext.Add(new Billing { Id = id, Address = address, Quantity = quantity, Amount = amount });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Billing '{id}' created. Waiting for Payment and Stock services.", id);

        await context.Publish<BillingPrepared>(new { context.CorrelationId });
    }
}
