using Microsoft.EntityFrameworkCore;

namespace MasterClassSagaPattern.Choregraphy.StockService;

public class StockDbContext : DbContext
{
    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Stock> Stocks { get; set; } = default!;
    public virtual DbSet<StockBooking> StockBookings { get; set; } = default!;
}
