public interface IMeterReadingValidator
{
    Task<bool> IsValidCsvAsync(string csvData);
}

