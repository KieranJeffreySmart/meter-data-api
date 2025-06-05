
using System.Runtime.CompilerServices;
namespace readingsapi;

public class Program
{

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Executing Meter Readings Appication");
        var taskName = Environment.GetEnvironmentVariable("TASK_NAME");

        var app = ApplicationFactory.CreateApplication(taskName);

        await app.RunAsync(args);

        Console.WriteLine("Exiting Meter Readings Appication");
    }
}

internal static class ApplicationFactory
{
    public static IMeterReadingsApplication CreateApplication(string? taskName)
    {
        if (!string.IsNullOrWhiteSpace(taskName) || string.Compare(taskName, "seed", StringComparison.OrdinalIgnoreCase) == 0)
        {
            Console.WriteLine("Starting task to seed data");
            return new MeterReadingsSeedDataTask();
        }

        return new MeterReadingsApi();
    }
}

public interface IMeterReadingsApplication
{
    public Task RunAsync(string[] args);
}
