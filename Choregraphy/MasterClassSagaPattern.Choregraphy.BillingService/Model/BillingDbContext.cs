using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.Choregraphy.BillingService;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Billing> Billings { get; set; } = default!;
}
