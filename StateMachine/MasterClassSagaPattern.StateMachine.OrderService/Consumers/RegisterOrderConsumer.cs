using MassTransit;
using MasterClassSagaPattern.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasterClassSagaPattern.StateMachine.OrderService;

public class RegisterOrderConsumer : IConsumer<RegisterOrder>
{
    private readonly OrderDbContext dbContext;
    private readonly ILogger<RegisterOrderConsumer> logger;

    public RegisterOrderConsumer(OrderDbContext dbContext, ILogger<RegisterOrderConsumer> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<RegisterOrder> context)
    {
        var id = context.CorrelationId.GetValueOrDefault();
        var quantity = context.Message.Quantity;
        var address = context.Message.Address;

        logger.LogInformation("Received {command} message with { Id = '{id}', Quantity = {quantity}, Address = '{address}' }", nameof(RegisterOrder), id, quantity, address);

        dbContext.Orders.Add(new Order { Id = id, Quantity = quantity, Address = address });

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Order '{id}' created.", id);

        await context.Publish<OrderRegistered>(new { context.CorrelationId, context.Message.Quantity, context.Message.Address });
    }
}
