using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.StockService
{
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
                logger.LogInformation($"'{id}' does exists in this context. Skipping.");

                return;
            }

            logger.LogInformation($"'{id}' exists in this context. Deleting because of {nameof(PaymentRefused)} message.");

            dbContext.StockBookings.Remove(stockBooking);

            var stock = await dbContext.Stocks.FirstOrDefaultAsync();

            stock.Quantity += stockBooking.Quantity;

            logger.LogInformation($"Stock is now {stock.Quantity}.");

            await dbContext.SaveChangesAsync();
        }
    }
}
