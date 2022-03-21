using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.StockService;

public class BookStockConsumer : IConsumer<BookStock>
{
    private readonly StockDbContext dbContext;
    private readonly ILogger<BookStockConsumer> logger;
    private static readonly SemaphoreSlim locker = new(1, 1);

    public BookStockConsumer(StockDbContext dbContext, ILogger<BookStockConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<BookStock> context)
    {
        await EnsureBaseCreatedAsync();

        var id = context.CorrelationId.GetValueOrDefault();
        var quantity = context.Message.Quantity;

        logger.LogInformation("Received {command} message with Id = '{id}' and Quantity = {quantity}.", nameof(BookStock), id, quantity);

        var couldEnterSemaphore = await locker.WaitAsync(TimeSpan.FromSeconds(30));

        if (!couldEnterSemaphore)
        {
            logger.LogError("Technical Error : couldn't enter thread-safe area.");

            await context.Publish<StockBookingFailed>(new { context.CorrelationId });

            return;
        }

        var stock = await dbContext.Stocks.FirstOrDefaultAsync();

        if (stock is null || stock.Quantity < quantity)
        {
            logger.LogInformation("Not enough stock left.");

            await context.Publish<StockBookingFailed>(new { context.CorrelationId });

            locker.Release();

            return;
        }

        logger.LogInformation("'{StockQuantity}' left in stock. Removing {quantity}.", stock.Quantity, quantity);

        stock.Quantity -= quantity;

        dbContext.StockBookings.Add(new StockBooking { Quantity = quantity, Id = id });

        await dbContext.SaveChangesAsync();

        await context.Publish<StockBooked>(new { context.CorrelationId });

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
