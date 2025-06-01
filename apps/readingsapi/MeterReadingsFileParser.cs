using System.Globalization;
using System.Text.RegularExpressions;

namespace readingsapi;

public partial class MeterReadingsFileParser
{
    public async IAsyncEnumerable<(bool err, MeterReading record)> ParseAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            yield break; // Skip empty files
        }

        using var reader = new StreamReader(file.OpenReadStream());
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {            
            var parts = line.Split(',');
            DateTimeFormatInfo dtfi = new DateTimeFormatInfo
            {
                ShortDatePattern = "dd/MM/yyyy",
                LongDatePattern = "dd/MM/yyyy HH:mm"
            };

            var r = MeterReadingValueRegex();

            if (!r.IsMatch(parts[2]))
            {
                yield return (true, new MeterReading(Guid.Empty, -1, DateTime.MinValue, -1)); // Skip invalid meter reading values
            }
            else
            {
                if (DateTime.TryParse(parts[1], dtfi, DateTimeStyles.None, out var dateTime))
                {
                    yield return (false, new MeterReading(Guid.NewGuid(), Convert.ToInt32(parts[0]), dateTime, Convert.ToInt32(parts[2])));
                }
                else
                {
                    yield return (true, new MeterReading(Guid.Empty, -1, DateTime.MinValue, -1)); // Skip invalid date formats
                }
            }
        }
    }

    [GeneratedRegex("^\\d{4}$")]
    private static partial Regex MeterReadingValueRegex();
}
