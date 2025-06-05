using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using readingsapi;
using readingsapi.adaptors;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string dbName = $"testDb_{Guid.NewGuid()}";
    private readonly Account[] accounts = [];


    // public CustomWebApplicationFactory(string dbName, Account[] accounts)
    // {
    //     this.dbName = dbName;
    //     this.accounts = accounts;
    // }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DB_CONNECTION_TYPE", "InMemory");
        Environment.SetEnvironmentVariable("DB_CONNECTION_NAME", dbName);
        builder.ConfigureServices(s =>
        {
            s.AddDbContext<MeterReadingsContext>(options =>
            {
                options.UseInMemoryDatabase(dbName)
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                    {
                        (context as MeterReadingsContext)?.Accounts.AddRange(accounts);
                        await context.SaveChangesAsync(cancellationToken);
                    });
            });
        });
    }
}


