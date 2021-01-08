using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.Orchestration.OrderService
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Order> Orders { get; set; }
    }
}
