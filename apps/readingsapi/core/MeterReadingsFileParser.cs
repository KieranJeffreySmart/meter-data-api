using System.Globalization;

namespace readingsapi;

public partial class MeterReadingsFileParser
{
    private readonly IMeterReadingValidator _validator;

    public MeterReadingsFileParser(IMeterReadingValidator validator)
    {
        _validator = validator;
    }

    public async IAsyncEnumerable<(bool err, NewMeterReadingDto record)> ParseAsync(Stream fileStream)
    {
        if (fileStream.Length == 0)
        {
            yield break; // Skip empty files
        }

        using var reader = new StreamReader(fileStream);

        string? line;
        var isFirstLine = true;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (isFirstLine && LineIsHeader(line))
            {
                isFirstLine = false;
                continue; // Skip header line
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue; // Skip empty lines
            }

            if (!await _validator.IsValidCsvAsync(line))
            {
                yield return (true, new NewMeterReadingDto(-1, DateTime.MinValue, -1));
            }
            else
            {
                var parts = line.Split(',');

                DateTimeFormatInfo dtfi = new DateTimeFormatInfo
                {
                    ShortDatePattern = "dd/MM/yyyy",
                    LongDatePattern = "dd/MM/yyyy HH:mm"
                };

                var dateTime = DateTime.Parse(parts[1], dtfi, DateTimeStyles.None);
                yield return (false, new NewMeterReadingDto(Convert.ToInt32(parts[0]), dateTime, Convert.ToInt32(parts[2])));
            }
        }
    }

    private static bool LineIsHeader(string line)
    {
        var parts = line.Split(',');

        return parts.Length >= 3 &&
                parts[0].Trim().Equals("AccountId", StringComparison.OrdinalIgnoreCase) &&
                parts[1].Trim().Equals("MeterReadingDateTime", StringComparison.OrdinalIgnoreCase) &&
                parts[2].Trim().Equals("MeterReadValue", StringComparison.OrdinalIgnoreCase);
    }
}
