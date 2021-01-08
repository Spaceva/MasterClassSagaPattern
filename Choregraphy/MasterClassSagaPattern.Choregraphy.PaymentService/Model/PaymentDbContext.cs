using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.Choregraphy.PaymentService
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Payment> Payments { get; set; }
    }
}
