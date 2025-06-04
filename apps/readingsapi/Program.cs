using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using readingsapi.adaptors;
using readingsapi.logging;

namespace readingsapi;

public class Program
{
    private static readonly string BAD_REQUEST_MESSAGE = "Bad Request: Invalid data provided.";
    private static readonly string METER_READINGS_URI_PATH = "/meter-reading-uploads";


    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var connectionType = Environment.GetEnvironmentVariable("DB_CONNECTION_TYPE");
        if (connectionType == "postgresdb")
        {
            builder.AddNpgsqlDbContext<MeterReadingsContext>(connectionName: "postgresdb");
        }
        else
        {
            var dbName = $"InMemDb_{Guid.NewGuid}";
            builder.Services.AddDbContext<MeterReadingsContext>(options => options.UseInMemoryDatabase(dbName));

            if (args.Length > 0 && string.Compare(args[0], "seed", StringComparison.OrdinalIgnoreCase) == 0)
            {
                SeedData(dbName);
            }
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

    private static void SeedData(string dbName)
    {
        var accounts = new List<Account>();
        using var reader = new StreamReader(new FileStream("./seed_data/Test_Accounts.csv", FileMode.Open, FileAccess.Read));
        string? line;
        bool skipHeader = true;
        while ((line = reader.ReadLine()) != null)
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

        var context = new MeterReadingsContext(
            new DbContextOptionsBuilder<MeterReadingsContext>()
                .UseInMemoryDatabase(dbName)
                .UseSeeding((context, _) =>
                    {
                        (context as MeterReadingsContext)?.Accounts.AddRange(accounts);
                        context.SaveChanges();
                    })
                .Options);

        context.Database.EnsureCreated();
    }

}
