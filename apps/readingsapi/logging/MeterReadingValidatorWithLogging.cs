namespace readingsapi.logging;

public class MeterReadingValidatorWithLogging : IMeterReadingValidator
{
    private readonly IMeterReadingValidator _inner;
    private readonly ILogger<IMeterReadingValidator> _logger;

    public MeterReadingValidatorWithLogging(IMeterReadingValidator inner, ILogger<IMeterReadingValidator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<bool> IsValidCsvAsync(string csvData)
    {
        _logger.LogInformation("Validating CSV data: {CsvData}", csvData);
        var isValid = await _inner.IsValidCsvAsync(csvData);
        _logger.LogInformation("Validation result: {IsValid}", isValid);
        return isValid;
    }
}