using System.Globalization;
using System.Text.RegularExpressions;

namespace readingsapi;

public partial class MeterReadingsFileParser
{
    // TODO: Consider changing this signature to use a Stream instead of IFormFile
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

            var parts = line.Split(',');
            // TODO: Validate the number of parts
            // TODO: Encapsulate and inject the validation logic

            DateTimeFormatInfo dtfi = new DateTimeFormatInfo
            {
                ShortDatePattern = "dd/MM/yyyy",
                LongDatePattern = "dd/MM/yyyy HH:mm"
            };

            var r = MeterReadingValueRegex();

            if (!r.IsMatch(parts[2]))
            {
                yield return (true, new NewMeterReadingDto(-1, DateTime.MinValue, -1)); // Skip invalid meter reading values
            }
            else
            {
                if (DateTime.TryParse(parts[1], dtfi, DateTimeStyles.None, out var dateTime))
                {
                    yield return (false, new NewMeterReadingDto(Convert.ToInt32(parts[0]), dateTime, Convert.ToInt32(parts[2])));
                }
                else
                {
                    yield return (true, new NewMeterReadingDto(-1, DateTime.MinValue, -1)); // Skip invalid date formats
                }
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

    [GeneratedRegex("^\\d{4}$")]
    private static partial Regex MeterReadingValueRegex();
}
