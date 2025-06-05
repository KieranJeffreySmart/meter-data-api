
namespace readingsapi;
public interface IMeterReadingWriteRepository
{
    Task Add(NewMeterReadingDto record);
    Task SaveAsync();
}

public interface IMeterReadingReadRepository
{
    Task<bool> Exists(int accountId, DateTime meterReadingDateTime);
}


public record MeterReading
{
    public Guid ReadingId { get; init; }
    public int AccountId { get; init; }
    public DateTime MeterReadingDateTime { get; init; }
    public int MeterReadValue { get; init; }

    public MeterReading(Guid readingId, int accountId, DateTime meterReadingDateTime, int meterReadValue)
    {
        ReadingId = readingId;
        AccountId = accountId;
        MeterReadingDateTime = meterReadingDateTime.ToUniversalTime();
        MeterReadValue = meterReadValue;
    }
}

public record NewMeterReadingDto(int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);