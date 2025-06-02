using Microsoft.EntityFrameworkCore;

namespace readingsapi.adaptors;

public class MeterReadingRepository : IMeterReadingWriteRepository, IMeterReadingReadRepository
{
    private readonly MeterReadingsContext _context;

    public MeterReadingRepository(MeterReadingsContext context)
    {
        _context = context;
    }

    public async Task Add(NewMeterReadingDto record)
    {
        await _context.Readings.AddAsync(new MeterReading(Guid.NewGuid(), record.AccountId, record.MeterReadingDateTime, record.MeterReadValue));
    }

    public async Task<bool> Exists(int accountId, DateTime meterReadingDateTime)
    {
        if (_context.Readings.Local.Any(r => r.AccountId == accountId && r.MeterReadingDateTime == meterReadingDateTime))
        {
            return true;
        }

        if (await _context.Readings.AnyAsync(r => r.AccountId == accountId && r.MeterReadingDateTime == meterReadingDateTime))
        {
            return true;
        }

        return false;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
