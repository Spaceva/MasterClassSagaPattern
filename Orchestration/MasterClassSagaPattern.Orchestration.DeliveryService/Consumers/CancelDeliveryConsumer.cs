using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.DeliveryService;

public class CancelDeliveryConsumer : IConsumer<CancelDelivery>
{
    private readonly DeliveryDbContext dbContext;
    private readonly ILogger<CancelDeliveryConsumer> logger;

    public CancelDeliveryConsumer(DeliveryDbContext dbContext, ILogger<CancelDeliveryConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<CancelDelivery> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var delivery = await dbContext.Deliveries.FindAsync(id);

        if (delivery is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {command} message.", id, nameof(CancelDelivery));

        dbContext.Deliveries.Remove(delivery);

        await dbContext.SaveChangesAsync();

        await context.Publish<DeliveryCancelled>(new { context.CorrelationId });
    }
}
