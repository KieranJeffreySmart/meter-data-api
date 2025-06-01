using System.Globalization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        _ = app.MapPost("/meter-reading-uploads", async (HttpRequest request, [FromServices] MeterReadingsContext dbContext) =>
        {
            await dbContext.Database.EnsureCreatedAsync();

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
                    if (file.Length == 0)
                    {
                        continue; // Skip empty files
                    }

                    using var reader = new StreamReader(file.OpenReadStream());
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var parts = line.Split(',');
                        DateTimeFormatInfo dtfi = new DateTimeFormatInfo
                        {
                            ShortDatePattern = "dd/MM/yyyy",
                            LongDatePattern = "dd/MM/yyyy HH:mm"
                        };

                        var reading = new Reading(Guid.NewGuid(), Convert.ToInt32(parts[0]), Convert.ToDateTime(parts[1], dtfi), Convert.ToInt32(parts[2]));
                        dbContext.Readings.Add(reading);
                    }
                }
                
                await dbContext.SaveChangesAsync();
            }
            catch (InvalidDataException _)
            {
                return Results.BadRequest("Bad Request: Invalid data provided.");
            }

            return Results.Ok("1");
        })
        .WithName("MeterReadingUploads");

        app.Run();
    }
}

public class MeterReadingsContext : DbContext
{
    public MeterReadingsContext(DbContextOptions<MeterReadingsContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Reading> Readings { get; set; }
}

public record Account(int AccountId, string FirstName, string LastName);

public record Reading(Guid ReadingId, int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);
