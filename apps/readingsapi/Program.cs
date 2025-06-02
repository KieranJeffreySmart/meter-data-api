using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using readingsapi.adaptors;

namespace readingsapi;

public class Program
{
    public static void Main(string[] args)
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
        builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
        builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        _ = app.MapPost("/meter-reading-uploads", async (HttpRequest request, [FromServices] IAccountsRepository accountsRepo, [FromServices] IMeterReadingRepository readingsRepo, [FromServices] MeterReadingsFileParser fileParser) =>
        {
            var successCount = 0;
            var failCount = 0;

            //TODO: Encapsulate and inject file management logic so it can be decoupled from the the http controller logic
            try
            {
                var formCollection = await request.ReadFormAsync();
                if (formCollection == null || formCollection.Files.Count == 0)
                {
                    return Results.BadRequest("Bad Request: Invalid data provided.");
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

                        var record = reading.record;

                        var accountExists = await accountsRepo.AccountExists(record.AccountId);

                        if (!accountExists)
                        {
                            // If the account does not exist, skip it
                            failCount++;
                            continue;
                        }

                        if (await readingsRepo.IsDuplicate(record))
                        {
                            // If the reading is a duplicate, skip it
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
