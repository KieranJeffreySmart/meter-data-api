using Microsoft.AspNetCore.Http.HttpResults;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapPost("/meter-reading-uploads", () =>
        {
            return Results.BadRequest("Bad Request: No data provided.");
        })
        .WithName("MeterReadingUploads");

        app.Run();
    }
}
