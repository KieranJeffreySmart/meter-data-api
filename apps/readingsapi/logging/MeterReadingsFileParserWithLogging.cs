namespace readingsapi.logging;

public class MeterReadingsFileParserWithLogging : IMeterReadingsFileParser
{
    private readonly IMeterReadingsFileParser _inner;
    private readonly ILogger<IMeterReadingsFileParser> _logger;

    public MeterReadingsFileParserWithLogging(IMeterReadingsFileParser inner, ILogger<IMeterReadingsFileParser> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async IAsyncEnumerable<(bool err, NewMeterReadingDto record)> ParseAsync(Stream fileStream)
    {
        await foreach (var result in _inner.ParseAsync(fileStream))
        {
            if (result.err)
            {
                _logger.LogWarning("Invalid meter reading data encountered.");
            }
            else
            {
                _logger.LogInformation("Parsed meter reading: {AccountId}, {DateTime}, {Value}", result.record.AccountId, result.record.MeterReadingDateTime, result.record.MeterReadValue);
            }

            yield return result;
        }
    }
}