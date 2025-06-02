namespace readingsapi.logging;
public class MeterReadingCsvFileProcessorWithLogging : IMeterReadingCsvFileProcessor
{
    private readonly IMeterReadingCsvFileProcessor _inner;
    private readonly ILogger<IMeterReadingCsvFileProcessor> _logger;

    public MeterReadingCsvFileProcessorWithLogging(IMeterReadingCsvFileProcessor inner, ILogger<IMeterReadingCsvFileProcessor> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ProcessingResults> ProcessFiles(IEnumerable<Stream> streams)
    {
        _logger.LogInformation("Starting processing of meter reading files");
        var results = await _inner.ProcessFiles(streams);
        _logger.LogInformation("Finished processing files: {SuccessCount} succeeded, {FailCount} failed", results.Succedded, results.Failed);
        return results;
    }
}