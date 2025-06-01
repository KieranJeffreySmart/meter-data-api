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
            var recordCount = 0;

            //TODO: Encapsulate and inject file management and database logic so it can be decoupled from the the http controller logic
            try
            {
                var formCollection = await request.ReadFormAsync();
                if (formCollection == null || formCollection.Files.Count == 0)
                {
                    // If no files are uploaded, return a Bad Request response
                    return Results.BadRequest("Bad Request: Invalid data provided.");
                }

                foreach (var file in formCollection.Files)
                {
                    await foreach (var reading in fileParser.ParseAsync(file))
                    {
                        if (reading.err)
                        {
                            // If the reading is invalid, skip it
                            continue;
                        }

                        if (dbContext.Accounts.Find(reading.record.AccountId) == null)
                        {
                            // If the account does not exist, skip the reading
                            continue;
                        }

                        dbContext.Readings.Add(reading.record);
                        recordCount++;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"Invalid data encountered: {ex.Message}");
                return Results.BadRequest("Bad Request: Invalid data provided.");
            }

            return Results.Ok(recordCount.ToString());
        })
        .WithName("MeterReadingUploads");

        app.Run();
    }
}

