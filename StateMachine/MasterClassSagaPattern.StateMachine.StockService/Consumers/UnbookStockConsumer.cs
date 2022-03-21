using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.StockService;

public class UnbookStockConsumer : IConsumer<UnbookStock>
{
    private readonly StockDbContext dbContext;
    private readonly ILogger<UnbookStockConsumer> logger;
    private static readonly SemaphoreSlim locker = new(1, 1);

    public UnbookStockConsumer(StockDbContext dbContext, ILogger<UnbookStockConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<UnbookStock> context)
    {
        await EnsureBaseCreatedAsync();

        var id = context.CorrelationId.GetValueOrDefault();
        var quantity = context.Message.Quantity;

        logger.LogInformation("Received {command} message with Id = '{id}' and Quantity = {quantity}.", nameof(UnbookStock), id, quantity);

        var couldEnterSemaphore = await locker.WaitAsync(TimeSpan.FromSeconds(30));

        if (!couldEnterSemaphore)
        {
            logger.LogError("Technical Error : couldn't enter thread-safe area.");

            await context.Publish<StockBookingFailed>(new { context.CorrelationId });

            return;
        }

        var stock = await dbContext.Stocks.FirstAsync();

        var booking = await dbContext.StockBookings.FindAsync(id);

        if (booking is null)
        {
            logger.LogInformation("No booking find with Id = '{id}'. Skipping.", id);

            locker.Release();

            return;
        }

        dbContext.StockBookings.Remove(booking);

        logger.LogInformation("'{StockQuantity}' left in stock. Adding {quantity} back.", stock.Quantity, quantity);

        stock.Quantity += quantity;

        await dbContext.SaveChangesAsync();

        locker.Release();
    }

    private async Task EnsureBaseCreatedAsync()
    {
        var stock = await dbContext.Stocks.FirstOrDefaultAsync();

        if (stock is not null)
        {
            return;
        }

        var initQuantity = 5;

        logger.LogInformation("Init stock quantity to {initQuantity}", initQuantity);

        await dbContext.Stocks.AddAsync(new Stock { Id = 1, Quantity = initQuantity });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Done.");
    }
}
