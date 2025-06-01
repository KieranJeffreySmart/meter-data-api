
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using readingsapi;
using Microsoft.Extensions.DependencyInjection;

internal class TestHelpers
{
    internal static IEnumerable<MeterReading> GetReadingsFromContext(string dbName)
    {
        var context = new MeterReadingsContext(
            new DbContextOptionsBuilder<MeterReadingsContext>()
                .UseInMemoryDatabase(dbName)
                .Options);
        return context.Readings.OrderBy(r => r.AccountId).ThenBy(r => r.MeterReadingDateTime);
    }

    internal static void AssertMeterReading(MeterReading actual, int accountId, DateTime meterReadingDateTime, int meterReadValue)
    {
        Assert.Equal(accountId, actual.AccountId);
        Assert.Equal(meterReadingDateTime, actual.MeterReadingDateTime);
        Assert.Equal(meterReadValue, actual.MeterReadValue);
    }

    internal static MultipartFormDataContent CreateFakeMultiPartFormData(string content)
    {
        ByteArrayContent byteContent = new(Encoding.UTF8.GetBytes(content));

        MultipartFormDataContent multipartContent = new() { { byteContent, "file", "readings.csv" } };
        return multipartContent;
    }

    internal static WebApplicationFactory<Program> CreateSeededWebFactory(WebApplicationFactory<Program> factory, Account[] accounts, string localDbName)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(s =>
            {
                s.AddDbContext<MeterReadingsContext>(options =>
                {
                    options.UseInMemoryDatabase(localDbName)
                    .UseAsyncSeeding(async (context, _, cancellationToken) =>
                    {
                        (context as MeterReadingsContext)?.Accounts.AddRange(accounts);
                        await context.SaveChangesAsync(cancellationToken);
                    });
                });
            });
        });
    }

    internal static async Task<WebApplicationFactory<Program>> CreateSeededWebFactoryAsync(WebApplicationFactory<Program> factory, string inportPath, string localDbName)
    {
        var accounts = new List<Account>();
        using var reader = new StreamReader(new FileStream(inportPath, FileMode.Open, FileAccess.Read));
        string? line;
        bool skipHeader = true;
        while ((line = await reader.ReadLineAsync()) != null)
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

        return CreateSeededWebFactory(factory, [.. accounts], localDbName);
    }
}