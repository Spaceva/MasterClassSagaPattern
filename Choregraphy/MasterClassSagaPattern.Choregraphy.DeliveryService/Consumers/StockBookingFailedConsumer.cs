using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService;

public class StockBookingFailedConsumer : IConsumer<StockBookingFailed>
{
    private readonly DeliveryDbContext dbContext;
    private readonly ILogger<StockBookingFailedConsumer> logger;

    public StockBookingFailedConsumer(DeliveryDbContext dbContext, ILogger<StockBookingFailedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<StockBookingFailed> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var reason = context.Message.Reason;
        var delivery = await dbContext.Deliveries.FindAsync(id);

        if (delivery is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {event} message with reason = '{reason}'.", id, nameof(StockBookingFailed), reason);

        dbContext.Deliveries.Remove(delivery);

        await dbContext.SaveChangesAsync();

        await context.Publish<DeliveryCancelled>(new { context.CorrelationId });
    }
}
