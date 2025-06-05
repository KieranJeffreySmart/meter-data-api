using Microsoft.EntityFrameworkCore;
using readingsapi.adaptors;

namespace readingsapi;

public class MeterReadingsSeedDataTask : IMeterReadingsApplication
{
    public async Task RunAsync(string[] args)
    {
        Console.WriteLine("Building task to seed data");
        var builder = WebApplication.CreateBuilder(args);

        //TODO: Refactor to use options and import a config object from environment variables
        var connectionType = Environment.GetEnvironmentVariable("DB_CONNECTION_TYPE");
        var connectionName = Environment.GetEnvironmentVariable("DB_CONNECTION_NAME") ?? "readingsdb";

        if (string.Compare(connectionType, "InMemory", StringComparison.OrdinalIgnoreCase) == 0 && !string.IsNullOrWhiteSpace(connectionName))
        {
            builder.Services.AddDbContext<MeterReadingsContext>(options =>
            {
                if (string.Compare(connectionType, "InMemory", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Console.WriteLine($"using inmemory db: {connectionName}");
                    options.UseInMemoryDatabase(connectionName);
                }
            });
        }
        else
        {
            builder.AddNpgsqlDbContext<MeterReadingsContext>(connectionName: connectionName);
        }
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            await app.ConfigureDbContextAsync();
            await app.SeedMeterReadingsAsync();
        }

        Environment.Exit(0);
    }
}
