using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.StockService
{
    public class PaymentCancelledConsumer : IConsumer<PaymentCancelled>
    {
        private readonly StockDbContext dbContext;
        private readonly ILogger<PaymentCancelledConsumer> logger;

        public PaymentCancelledConsumer(StockDbContext dbContext, ILogger<PaymentCancelledConsumer> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCancelled> context)
        {
            var id = context.CorrelationId.GetValueOrDefault();
            var stockBooking = await dbContext.StockBookings.FindAsync(id);

            if (stockBooking is null)
            {
                logger.LogInformation($"'{id}' does exists in this context. Skipping.");

                return;
            }

            logger.LogInformation($"'{id}' exists in this context. Deleting because of {nameof(PaymentCancelled)} message.");

            dbContext.StockBookings.Remove(stockBooking);

            var stock = await dbContext.Stocks.FirstOrDefaultAsync();

            stock.Quantity += stockBooking.Quantity;

            logger.LogInformation($"Stock is now {stock.Quantity}.");

            await dbContext.SaveChangesAsync();
        }
    }
}
