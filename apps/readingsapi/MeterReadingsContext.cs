using Microsoft.EntityFrameworkCore;

namespace readingsapi;

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

public record Account(int AccountId, string FirstName, string LastName);

public record MeterReading(Guid ReadingId, int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);