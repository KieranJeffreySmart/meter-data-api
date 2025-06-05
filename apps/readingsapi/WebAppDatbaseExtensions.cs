using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using readingsapi.adaptors;

namespace readingsapi;

public static class WebAppDatbaseExtensions
{
    public static async Task EnsureDatabaseAsync(MeterReadingsContext dbContext)
    {
        if (dbContext.Database.IsRelational())
        {
            var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();
            var createStrategy = dbContext.Database.CreateExecutionStrategy();
            await createStrategy.ExecuteAsync(async () =>
            {
                if (!await dbCreator.ExistsAsync())
                {
                    await dbCreator.CreateAsync();
                }
            });
        }
    }

    const string DEFAULT_SEED_DATA = @"AccountId,FirstName,LastName
2344,Tommy,Test
2233,Barry,Test
8766,Sally,Test
2345,Jerry,Test
2346,Ollie,Test
2347,Tara,Test
2348,Tammy,Test
2349,Simon,Test
2350,Colin,Test
2351,Gladys,Test
2352,Greg,Test
2353,Tony,Test
2355,Arthur,Test
2356,Craig,Test
6776,Laura,Test
4534,JOSH,TEST
1234,Freya,Test
1239,Noddy,Test
1240,Archie,Test
1241,Lara,Test
1242,Tim,Test
1243,Graham,Test
1244,Tony,Test
1245,Neville,Test
1246,Jo,Test
1247,Jim,Test
1248,Pam,Test
";

    public static async Task ConfigureDbContextAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeterReadingsContext>();

        await EnsureDatabaseAsync(context);

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }
        });
    }
    
    public static async Task SeedMeterReadingsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeterReadingsContext>();
        Console.WriteLine("Sededing database context...");

        await EnsureDatabaseAsync(context);
        await context.Database.EnsureCreatedAsync();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {

            //TODO: Refactor this into something testable
            var accounts = new List<Account>();
            using var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(DEFAULT_SEED_DATA)));
            string? line;
            bool skipHeader = true;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (skipHeader)
                {
                    skipHeader = false;
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length < 2)
                {
                    continue; // Skip invalid lines
                }

                int accountId = Convert.ToInt32(parts[0]);
                string firstName = parts[1];
                string lastName = parts.Length > 2 ? parts[2] : string.Empty;

                accounts.Add(new Account(accountId, firstName, lastName));
            }

            Console.WriteLine($"Seeding {accounts.Count} accounts into the database...");

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        });
        Console.WriteLine("Finished seeding database.");
    }
}
