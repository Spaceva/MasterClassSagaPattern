using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.DeliveryService;

public class PaymentAcceptedConsumer : IConsumer<PaymentAccepted>
{
    private readonly DeliveryDbContext dbContext;
    private readonly ILogger<PaymentAcceptedConsumer> logger;

    public PaymentAcceptedConsumer(DeliveryDbContext dbContext, ILogger<PaymentAcceptedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentAccepted> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var delivery = await dbContext.Deliveries.FindAsync(id);

        logger.LogInformation("Received {event} message with Id = '{id}'", nameof(PaymentAccepted), id);

        if (delivery is null)
        {
            logger.LogInformation("'{id}' does not exists in this context. Rejecting, will retry in a few.", id);

            throw new DeliveryNotFoundException();
        }

        logger.LogInformation("'{id}' exists in this context.", id);

        delivery.IsPaymentAccepted = true;
        var shouldSendIsShippedMessage = false;

        if (delivery.IsBillingCompleted
            && delivery.IsPaymentAccepted
            && delivery.IsStockBooked)
        {
            logger.LogInformation("'{id}' can be shipped !", id);

            delivery.IsShipped = true;
            shouldSendIsShippedMessage = true;
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation("'{id}' updated.", id);

        if (shouldSendIsShippedMessage)
        {
            await context.Publish<DeliveryStarted>(new { CorrelationId = id });
        }
    }
}
