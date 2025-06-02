using System.Globalization;
using System.Text.RegularExpressions;

namespace readingsapi;

public partial class MeterReadingValidator : IMeterReadingValidator
{
    readonly IAccountsRepository _accountsRepo;
    readonly IMeterReadingReadRepository _meterReadingReadRepo;

    public MeterReadingValidator(IAccountsRepository accountsRepo, IMeterReadingReadRepository meterReadingReadRepo)
    {
        _accountsRepo = accountsRepo;
        _meterReadingReadRepo = meterReadingReadRepo;
    }

    public async Task<bool> IsValidCsvAsync(string csvData)
    {
        // Validate missing data
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

        // Validate meter reading value
        var r = MeterReadingValueRegex();

        if (!r.IsMatch(parts[2]))
        {
            return false;
        }

        // Validate meter reading date time
        DateTimeFormatInfo dtfi = new DateTimeFormatInfo
        {
            ShortDatePattern = "dd/MM/yyyy",
            LongDatePattern = "dd/MM/yyyy HH:mm"
        };

        if (!DateTime.TryParse(parts[1], dtfi, DateTimeStyles.None, out var dateTime))
        {
            return false;
        }

        // Validate account number
        if (!int.TryParse(parts[0], out var accountNumber)
            || accountNumber <= 0
            || !await _accountsRepo.AccountExists(accountNumber))
        {
            return false;
        }

        if (await _meterReadingReadRepo.Exists(accountNumber, dateTime))
        {
            return false;
        }

        return true;
    }

    [GeneratedRegex("^\\d{4}$")]
    private static partial Regex MeterReadingValueRegex();
}
