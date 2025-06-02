
public interface IMeterReadingRepository
{
    Task<bool> IsDuplicate(NewMeterReadingDto record);

    Task Add(NewMeterReadingDto record);
    Task SaveAsync();
}


public record MeterReading(Guid ReadingId, int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);

public record NewMeterReadingDto(int AccountId, DateTime MeterReadingDateTime, int MeterReadValue);