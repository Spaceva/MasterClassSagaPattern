using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.StateMachine.StockService
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Stock> Stocks { get; set; }
    }
}
