
namespace readingsapi;
public interface IAccountsRepository
{
    Task<bool> AccountExists(int accountId);
}

public record Account(int AccountId, string FirstName, string LastName);
