using Microsoft.EntityFrameworkCore;

namespace readingsapi.adaptors;

public class MeterReadingsContext : DbContext
{
    public MeterReadingsContext(DbContextOptions<MeterReadingsContext> options) : base(options)
    {
    }

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(a => a.AccountId);
        modelBuilder.Entity<MeterReading>().HasKey(mr => mr.ReadingId);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<MeterReading> Readings { get; set; }
}
