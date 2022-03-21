using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.StateMachine.OrderService;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; } = default!;
}
