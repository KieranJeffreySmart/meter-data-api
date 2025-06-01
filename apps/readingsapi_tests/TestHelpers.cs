
using System.Text;
using Microsoft.EntityFrameworkCore;
using readingsapi;

internal class TestHelpers
{
    internal static IEnumerable<MeterReading> GetReadingsFromContext(string dbName)
    {
        var context = new MeterReadingsContext(
            new DbContextOptionsBuilder<MeterReadingsContext>()
                .UseInMemoryDatabase(dbName)
                .Options);
        return context.Readings.OrderBy(r => r.MeterReadingDateTime);
    }

    internal static void AssertMeterReading(MeterReading actual, int accountId, DateTime meterReadingDateTime, int meterReadValue)
    {
        Assert.Equal(accountId, actual.AccountId);
        Assert.Equal(meterReadingDateTime, actual.MeterReadingDateTime);
        Assert.Equal(meterReadValue, actual.MeterReadValue);
    }

    internal static MultipartFormDataContent CreateFakeMultiPartFormData(string content)
    {
        ByteArrayContent byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));

        MultipartFormDataContent multipartContent = new MultipartFormDataContent { { byteContent, "file", "readings.csv" } };
        return multipartContent;
    }

}