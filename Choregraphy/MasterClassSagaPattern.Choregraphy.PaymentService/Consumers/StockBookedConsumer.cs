using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.PaymentService;

public class StockBookedConsumer : IConsumer<StockBooked>
{
    private readonly PaymentDbContext dbContext;
    private readonly ILogger<StockBookedConsumer> logger;

    public StockBookedConsumer(PaymentDbContext dbContext, ILogger<StockBookedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<StockBooked> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var payment = await dbContext.Payments.FindAsync(id);

        logger.LogInformation("Received {event} message with Id = '{id}'", nameof(StockBooked), id);

        if (payment is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new PaymentNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context.", id);

        payment.IsStockBooked = true;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("'{id}' updated.", id);
    }
}
