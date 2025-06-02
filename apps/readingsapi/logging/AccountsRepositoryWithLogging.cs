namespace readingsapi.logging;

public class AccountsRepositoryWithLogging : IAccountsRepository
{
    private readonly IAccountsRepository _repository;
    private readonly ILogger<IAccountsRepository> _logger;

    public AccountsRepositoryWithLogging(IAccountsRepository repository, ILogger<IAccountsRepository> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> AccountExists(int accountId)
    {
        _logger.LogInformation("Checking if account exists: {AccountId}", accountId);
        var exists = await _repository.AccountExists(accountId);
        _logger.LogInformation("Account exists: {Exists}", exists);
        return exists;
    }
}