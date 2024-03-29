﻿using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Orchestration.StockService;

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

        logger.LogInformation($"Received {nameof(UnbookStock)} message with Id = '{id}' and Quantity = {quantity}.");

        var couldEnterSemaphore = await locker.WaitAsync(TimeSpan.FromSeconds(30));

        if (!couldEnterSemaphore)
        {
            logger.LogError($"Technical Error : couldn't enter thread-safe area. Will retry.");

            throw new Exception();
        }

        var stock = await dbContext.Stocks.FirstAsync();

        var booking = await dbContext.StockBookings.FindAsync(id);

        if (booking is null)
        {
            logger.LogInformation($"No booking find with Id = '{id}'. Skipping.");

            locker.Release();

            return;
        }

        dbContext.StockBookings.Remove(booking);

        logger.LogInformation($"'{stock.Quantity}' left in stock. Adding {quantity} back.");

        stock.Quantity += quantity;

        await dbContext.SaveChangesAsync();

        locker.Release();
    }

    private async Task EnsureBaseCreatedAsync()
    {
        var stock = await dbContext.Stocks.FirstOrDefaultAsync();

        if (!(stock is null))
        {
            return;
        }

        var initQuantity = 5;

        logger.LogInformation($"Init stock quantity to {initQuantity}");

        await dbContext.Stocks.AddAsync(new Stock { Id = 1, Quantity = initQuantity });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Done.");
    }
}
