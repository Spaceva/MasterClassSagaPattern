using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.MainUI
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions<MainDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Payment> Payments { get; set; }
    }
}
