using Microsoft.EntityFrameworkCore;

namespace readingsapi.adaptors;

public class MeterReadingRepository : IMeterReadingRepository
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

    public async Task<bool> IsDuplicate(NewMeterReadingDto record)
    {
        if (_context.Readings.Local.Any(r => r.AccountId == record.AccountId && r.MeterReadingDateTime == record.MeterReadingDateTime))
        {
            return true;
        }

        if (await _context.Readings.AnyAsync(r => r.AccountId == record.AccountId && r.MeterReadingDateTime == record.MeterReadingDateTime))
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
