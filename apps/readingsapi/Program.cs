using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using readingsapi.adaptors;

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
            builder.Services.AddDbContext<MeterReadingsContext>(options => options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid}"));
        }

        builder.AddServiceDefaults();

        builder.Services.AddScoped<MeterReadingsFileParser>();
        builder.Services.AddScoped<IMeterReadingValidator, MeterReadingValidator>();
        builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
        builder.Services.AddScoped<IMeterReadingWriteRepository, MeterReadingRepository>();
        builder.Services.AddScoped<IMeterReadingReadRepository, MeterReadingRepository>();

        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        _ = app.MapPost(METER_READINGS_URI_PATH, async (HttpRequest request, [FromServices] IMeterReadingWriteRepository readingsRepo, [FromServices] MeterReadingsFileParser fileParser) =>
        {
            var successCount = 0;
            var failCount = 0;

            //TODO: Encapsulate and inject file management logic so it can be decoupled from the the http controller logic
            try
            {
                var formCollection = await request.ReadFormAsync();
                if (formCollection == null || formCollection.Files.Count == 0)
                {
                    return Results.BadRequest(BAD_REQUEST_MESSAGE);
                }

                foreach (var file in formCollection.Files)
                {
                    await foreach (var reading in fileParser.ParseAsync(file.OpenReadStream()))
                    {
                        if (reading.err)
                        {
                            // If the reading is invalid, skip it
                            failCount++;
                            continue;
                        }

                        await readingsRepo.Add(reading.record);
                        successCount++;
                    }
                }

                await readingsRepo.SaveAsync();
            }
            catch (InvalidDataException ex)
            {
                LogException(ex);
                return Results.BadRequest(BAD_REQUEST_MESSAGE);
            }

            return Results.Ok(new ProcessingResults(successCount, failCount));
        });

        app.Run();
    }

    private static void LogException(Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
    }

    private record ProcessingResults(int Succedded, int Failed);
}
