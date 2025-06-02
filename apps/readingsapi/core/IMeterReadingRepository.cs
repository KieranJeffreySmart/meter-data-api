
public interface IMeterReadingWriteRepository
{
    Task Add(NewMeterReadingDto record);
    Task SaveAsync();
}

public interface IMeterReadingReadRepository
{
    Task<bool> Exists(int accountId, DateTime meterReadingDateTime);
}


public record MeterReading(Guid ReadingId, int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);

public record NewMeterReadingDto(int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);