
namespace readingsapi;
public interface IMeterReadingCsvFileProcessor
{
    Task<ProcessingResults> ProcessFiles(IEnumerable<Stream> enumerable);
}

public class MeterReadingCsvFileProcessor : IMeterReadingCsvFileProcessor
{
    private readonly IMeterReadingWriteRepository _writeRepository;
    private readonly IMeterReadingsFileParser _fileParser;

    public MeterReadingCsvFileProcessor(IMeterReadingWriteRepository writeRepository, IMeterReadingsFileParser fileParser)
    {
        _writeRepository = writeRepository;
        _fileParser = fileParser;
    }

    public async Task<ProcessingResults> ProcessFiles(IEnumerable<Stream> streams)
    {
        var successCount = 0;
        var failCount = 0;
        foreach (var stream in streams)
        {
            await foreach (var reading in _fileParser.ParseAsync(stream))
            {
                if (reading.err)
                {
                    // If the reading is invalid, skip it
                    failCount++;
                    continue;
                }

                await _writeRepository.Add(reading.record);
                successCount++;
            }
        }

        await _writeRepository.SaveAsync();

        return new ProcessingResults(successCount, failCount);
    }
}

public record ProcessingResults(int Succedded, int Failed);
