using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.PaymentService;

public class StockBookingFailedConsumer : IConsumer<StockBookingFailed>
{
    private readonly PaymentDbContext dbContext;
    private readonly ILogger<StockBookingFailedConsumer> logger;

    public StockBookingFailedConsumer(PaymentDbContext dbContext, ILogger<StockBookingFailedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<StockBookingFailed> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var reason = context.Message.Reason;
        var payment = await dbContext.Payments.FindAsync(id);

        if (payment is null)
        {
            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {event} message with reason = '{reason}'.", id, nameof(StockBookingFailed), reason);

        dbContext.Payments.Remove(payment);

        await dbContext.SaveChangesAsync();

        await context.Publish<PaymentCancelled>(new { context.CorrelationId });
    }
}
