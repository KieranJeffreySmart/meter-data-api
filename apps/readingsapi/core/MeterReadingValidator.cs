using System.Globalization;
using System.Text.RegularExpressions;

namespace readingsapi;

public partial class MeterReadingValidator : IMeterReadingValidator
{
    public async Task<bool> IsValidCsvAsync(string csvData)
    {
        if (string.IsNullOrWhiteSpace(csvData))
        {
            return false;
        }

        var parts = csvData.Split(',');
        if (parts.Length < 3)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(parts[0])
            || string.IsNullOrWhiteSpace(parts[1])
            || string.IsNullOrWhiteSpace(parts[2]))
        {
            return false;
        }

        var r = MeterReadingValueRegex();

        if (!r.IsMatch(parts[2]))
        {
            return false;
        }
        
        DateTimeFormatInfo dtfi = new DateTimeFormatInfo
        {
            ShortDatePattern = "dd/MM/yyyy",
            LongDatePattern = "dd/MM/yyyy HH:mm"
        };

        if (!DateTime.TryParse(parts[1], dtfi, DateTimeStyles.None, out var dateTime))
        {
            return false;
        }

        return true;
    }

    [GeneratedRegex("^\\d{4}$")]
    private static partial Regex MeterReadingValueRegex();
}
