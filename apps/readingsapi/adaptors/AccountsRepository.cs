using Microsoft.EntityFrameworkCore;

namespace readingsapi.adaptors;

public class AccountsRepository : IAccountsRepository
{
    private readonly MeterReadingsContext _context;

    public AccountsRepository(MeterReadingsContext context)
    {
        _context = context;
    }

    public async Task<bool> AccountExists(int accountId)
    {
        return await _context.Accounts.AnyAsync(a => a.AccountId == accountId);
    }
}