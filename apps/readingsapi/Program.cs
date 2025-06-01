using System.Globalization;
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
                        dbContext.Readings.Add(reading);
                        recordCount++;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (InvalidDataException _)
            {
                return Results.BadRequest("Bad Request: Invalid data provided.");
            }

            return Results.Ok(recordCount.ToString());
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

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(a => a.AccountId);
        modelBuilder.Entity<MeterReading>().HasKey(mr => mr.ReadingId);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<MeterReading> Readings { get; set; }
}

public class MeterReadingsFileParser
{
    public async IAsyncEnumerable<MeterReading> ParseAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            yield break; // Skip empty files
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

            yield return new MeterReading(Guid.NewGuid(), Convert.ToInt32(parts[0]), Convert.ToDateTime(parts[1], dtfi), Convert.ToInt32(parts[2]));
        }
    }
}

public record Account(int AccountId, string FirstName, string LastName);

public record MeterReading(Guid ReadingId, int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);
