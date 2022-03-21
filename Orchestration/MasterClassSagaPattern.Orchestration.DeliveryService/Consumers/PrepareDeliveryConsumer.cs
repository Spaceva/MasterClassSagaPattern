using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.DeliveryService;

public class PrepareDeliveryConsumer : IConsumer<PrepareDelivery>
{
    private readonly DeliveryDbContext dbContext;
    private readonly ILogger<PrepareDeliveryConsumer> logger;

    public PrepareDeliveryConsumer(DeliveryDbContext dbContext, ILogger<PrepareDeliveryConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PrepareDelivery> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var address = context.Message.Address;

        logger.LogInformation("Received {command} message with Id = '{id}' and Address = '{address}'.", nameof(PrepareDelivery), id, address);

        dbContext.Add(new Delivery { Id = id, Address = address });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Delivery '{id}' created. Waiting for Payment, Stock and Billing services.", id);

        await context.Publish<DeliveryPrepared>(new { context.CorrelationId });
    }
}
