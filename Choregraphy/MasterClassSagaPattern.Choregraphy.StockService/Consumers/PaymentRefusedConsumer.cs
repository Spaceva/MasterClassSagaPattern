using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.StockService;

public class PaymentRefusedConsumer : IConsumer<PaymentRefused>
{
    private readonly StockDbContext dbContext;
    private readonly ILogger<PaymentRefusedConsumer> logger;

    public PaymentRefusedConsumer(StockDbContext dbContext, ILogger<PaymentRefusedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentRefused> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var stockBooking = await dbContext.StockBookings.FindAsync(id);

        if (stockBooking is null)
        {
            logger.LogInformation("'{id}' does exists in this context. Skipping.", id);

            return;
        }

        logger.LogInformation("'{id}' exists in this context. Deleting because of {nameof(PaymentRefused)} message.", id, nameof(PaymentRefused));

        dbContext.StockBookings.Remove(stockBooking);

        var stock = await dbContext.Stocks.FirstOrDefaultAsync();

        if (stock is null)
        {
            var initQuantity = 5;

            logger.LogInformation("Init stock quantity to {initQuantity}", initQuantity);

            await dbContext.Stocks.AddAsync(new Stock { Id = 1, Quantity = initQuantity });

            await dbContext.SaveChangesAsync();

            return;
        }

        stock.Quantity += stockBooking.Quantity;

        logger.LogInformation("Stock is now {Quantity}.", stock.Quantity);

        await dbContext.SaveChangesAsync();
    }
}
