using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.StateMachine.PaymentService;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Payment> Payments { get; set; } = default!;
}
