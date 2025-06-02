using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace readingsapi;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<MeterReadingsContext>(
            options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
            );

        builder.Services.AddScoped<MeterReadingsFileParser>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        _ = app.MapPost("/meter-reading-uploads", async (HttpRequest request, [FromServices] MeterReadingsContext dbContext, [FromServices] MeterReadingsFileParser fileParser) =>
        {
            await dbContext.Database.EnsureCreatedAsync();
            var successCount = 0;
            var failCount = 0;

            //TODO: Encapsulate and inject file management and database logic so it can be decoupled from the the http controller logic
            try
            {
                var formCollection = await request.ReadFormAsync();
                if (formCollection == null || formCollection.Files.Count == 0)
                {
                    return Results.BadRequest("Bad Request: Invalid data provided.");
                }

                foreach (var file in formCollection.Files)
                {
                    await foreach (var reading in fileParser.ParseAsync(file))
                    {
                        if (reading.err)
                        {
                            // If the reading is invalid, skip it
                            failCount++;
                            continue;
                        }

                        if (await dbContext.Accounts.FindAsync(reading.record.AccountId) == null)
                        {
                            // If the account does not exist, skip it
                            failCount++;
                            continue;
                        }

                        if (dbContext.Readings.Local.Any(r => r.AccountId == reading.record.AccountId && r.MeterReadingDateTime == reading.record.MeterReadingDateTime))
                        {
                            // If the reading already exists locally, skip it
                            failCount++;
                            continue;
                        }

                        if (await dbContext.Readings.AnyAsync(r => r.AccountId == reading.record.AccountId && r.MeterReadingDateTime == reading.record.MeterReadingDateTime))
                        {
                            // If the reading already exists in the database, skip it
                            failCount++;
                            continue;
                        }

                        dbContext.Readings.Add(reading.record);
                        successCount++;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Invalid data encountered: {ex.Message}");
                return Results.BadRequest("Bad Request: Invalid data provided.");
            }

            return Results.Ok(new ProcessingResults(successCount, failCount));
        })
        .WithName("MeterReadingUploads");

        app.Run();
    }

    private record ProcessingResults(int Succedded, int Failed);
}
