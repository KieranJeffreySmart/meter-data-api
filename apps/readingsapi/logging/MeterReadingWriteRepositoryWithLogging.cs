namespace readingsapi.logging;

public class MeterReadingWriteRepositoryWithLogging : IMeterReadingWriteRepository
{
    private readonly IMeterReadingWriteRepository _writeRepository;
    private readonly ILogger<IMeterReadingWriteRepository> _writeLogger;

    public MeterReadingWriteRepositoryWithLogging(IMeterReadingWriteRepository writeRepository, ILogger<IMeterReadingWriteRepository> writeLogger)
    {
        _writeRepository = writeRepository;
        _writeLogger = writeLogger;
    }

    public async Task Add(NewMeterReadingDto record)
    {
        _writeLogger.LogInformation("Adding new meter reading for account {AccountId} at {DateTime}", record.AccountId, record.MeterReadingDateTime);
        await _writeRepository.Add(record);
        _writeLogger.LogInformation("Added new meter reading for account {AccountId} at {DateTime}", record.AccountId, record.MeterReadingDateTime);
    }

    public async Task SaveAsync()
    {
        _writeLogger.LogInformation("Saving changes to meter readings");
        await _writeRepository.SaveAsync();
        _writeLogger.LogInformation("Changes saved successfully");
    }
}
