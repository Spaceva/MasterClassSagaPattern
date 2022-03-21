using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.Choregraphy.OrderService;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly OrderDbContext dbContext;
    private readonly ILogger<OrderCreatedConsumer> logger;

    public OrderCreatedConsumer(OrderDbContext dbContext, ILogger<OrderCreatedConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var quantity = context.Message.Quantity;
        var address = context.Message.Address;

        logger.LogInformation("Received {event} message with {{ Id = '{id}', Quantity = {quantity}, Address = '{address}' }}", nameof(OrderCreated), id, quantity, address);

        dbContext.Orders.Add(new Order { Id = id, Quantity = quantity, Address = address });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Order '{id}' created.", id);

        await context.Publish<OrderRegistered>(new { context.CorrelationId, context.Message.Quantity, context.Message.Address });
    }
}
