using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using readingsapi.adaptors;
using readingsapi.logging;

namespace readingsapi;

public class Program
{
    private static readonly string BAD_REQUEST_MESSAGE = "Bad Request: Invalid data provided.";
    private static readonly string METER_READINGS_URI_PATH = "/meter-reading-uploads";


    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //TODO: Refactor to use .net options and import environment variables
        var connectionType = Environment.GetEnvironmentVariable("DB_CONNECTION_TYPE");
        var connectionName = Environment.GetEnvironmentVariable("DB_CONNECTION_NAME");

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
            builder.AddNpgsqlDbContext<MeterReadingsContext>(connectionName: "readingsdb");
        }

        builder.AddServiceDefaults();
        builder.Services.AddScoped<MeterReadingsFileParser>();
        builder.Services.AddScoped<MeterReadingValidator>();
        builder.Services.AddScoped<AccountsRepository>();
        builder.Services.AddScoped<MeterReadingRepository>();
        builder.Services.AddScoped<MeterReadingRepository>();
        builder.Services.AddScoped<MeterReadingCsvFileProcessor>();

        builder.Services.AddLoggingDecorator<IMeterReadingsFileParser, MeterReadingsFileParser>((inner, logger) => new MeterReadingsFileParserWithLogging(inner, logger));
        builder.Services.AddLoggingDecorator<IMeterReadingValidator, MeterReadingValidator>((inner, logger) => new MeterReadingValidatorWithLogging(inner, logger));
        builder.Services.AddLoggingDecorator<IAccountsRepository, AccountsRepository>((inner, logger) => new AccountsRepositoryWithLogging(inner, logger));
        builder.Services.AddLoggingDecorator<IMeterReadingWriteRepository, MeterReadingRepository>((inner, logger) => new MeterReadingWriteRepositoryWithLogging(inner, logger));
        builder.Services.AddLoggingDecorator<IMeterReadingReadRepository, MeterReadingRepository>((inner, logger) => new MeterReadingReadRepositoryWithLogging(inner, logger));
        builder.Services.AddLoggingDecorator<IMeterReadingCsvFileProcessor, MeterReadingCsvFileProcessor>((inner, logger) => new MeterReadingCsvFileProcessorWithLogging(inner, logger));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            Console.WriteLine("configure for development");
            await app.ConfigureDbContextAsync();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        _ = app.MapPost(METER_READINGS_URI_PATH, async (HttpRequest request, [FromServices] IMeterReadingCsvFileProcessor fileProcessor) =>
        {
            try
            {
                var formCollection = await request.ReadFormAsync();
                if (formCollection == null || formCollection.Files.Count == 0)
                {
                    return Results.BadRequest(BAD_REQUEST_MESSAGE);
                }

                return Results.Ok(await fileProcessor.ProcessFiles(formCollection.Files.Select(f => f.OpenReadStream())));
            }
            catch (InvalidDataException ex)
            {
                LogException(ex);
                return Results.BadRequest(BAD_REQUEST_MESSAGE);
            }
        });

        app.Run();
    }

    private static void LogException(Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
    }

}


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
                Console.WriteLine("Ensuring database exists...");
                if (!await dbCreator.ExistsAsync())
                {
                    Console.WriteLine("Database does not exist, creating...");
                    await dbCreator.CreateAsync();
                }
                else
                {
                    Console.WriteLine("Database already exists.");
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

    private static async Task SeedData(this WebApplication app)
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

    public static async Task ConfigureDbContextAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MeterReadingsContext>();
        Console.WriteLine("Configuring database context...");

        await EnsureDatabaseAsync(context);

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (context.Database.IsRelational())
            {
                Console.WriteLine("Migrating database...");
                await context.Database.MigrateAsync();                
                Console.WriteLine("Finished Migrating database.");
            }
        });
    }
}
