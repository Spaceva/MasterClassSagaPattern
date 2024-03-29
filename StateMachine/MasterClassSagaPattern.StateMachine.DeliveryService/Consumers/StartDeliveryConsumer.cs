﻿using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.DeliveryService;

public class StartDeliveryConsumer : IConsumer<StartDelivery>
{
    private readonly DeliveryDbContext dbContext;
    private readonly ILogger<StartDeliveryConsumer> logger;

    public StartDeliveryConsumer(DeliveryDbContext dbContext, ILogger<StartDeliveryConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<StartDelivery> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();

        logger.LogInformation("Received {command} message with Id = '{id}'", nameof(StartDelivery), id);

        var delivery = await dbContext.Deliveries.FindAsync(id);

        if (delivery is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new DeliveryNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context.", id);

        delivery.IsShipped = true;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("'{id}' updated.", id);

        await context.Publish<DeliveryStarted>(new { context.CorrelationId });
    }
}
