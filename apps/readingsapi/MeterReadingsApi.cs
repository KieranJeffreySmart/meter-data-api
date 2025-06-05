using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using readingsapi.adaptors;
using readingsapi.logging;

namespace readingsapi;

public class MeterReadingsApi : IMeterReadingsApplication
{
    private static readonly string BAD_REQUEST_MESSAGE = "Bad Request: Invalid data provided.";
    private static readonly string METER_READINGS_URI_PATH = "/meter-reading-uploads";
    public async Task RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

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
                // TODO: Replace this with a default error handler in the controller pipeline
                Console.Error.WriteLine($"Error: {ex.Message}");
                return Results.BadRequest(BAD_REQUEST_MESSAGE);
            }
        });

        app.Run();
    }
}
