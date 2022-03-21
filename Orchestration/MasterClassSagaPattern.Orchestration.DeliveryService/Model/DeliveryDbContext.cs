using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.Orchestration.DeliveryService;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Delivery> Deliveries { get; set; } = default!;
}
