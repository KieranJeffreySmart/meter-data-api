namespace readingsapi.logging;

public class MeterReadingReadRepositoryWithLogging : IMeterReadingReadRepository
{
    private readonly IMeterReadingReadRepository _readRepository;
    private readonly ILogger<IMeterReadingReadRepository> _readLogger;

    public MeterReadingReadRepositoryWithLogging(IMeterReadingReadRepository readRepository, ILogger<IMeterReadingReadRepository> readLogger)
    {
        _readRepository = readRepository;
        _readLogger = readLogger;
    }

    public async Task<bool> Exists(int accountId, DateTime meterReadingDateTime)
    {
        _readLogger.LogInformation("Checking if meter reading exists for account {AccountId} at {DateTime}", accountId, meterReadingDateTime);
        var exists = await _readRepository.Exists(accountId, meterReadingDateTime);
        _readLogger.LogInformation("Meter reading exists: {Exists}", exists);
        return exists;
    }
}
