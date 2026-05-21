using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IAccountService
    {
        Task<string> RegisterAsync(RegisterModel model);

        Task<string> LoginAsync(LoginModel model);

        Task<Account?> GetAccountByEmailAsync(string email);

        Task<Account?> GetAccountByIdAsync(int accountId);

        Task<List<Account>> GetAllAccountsAsync();

        Task<string> UpdateAsync(int accountId, RegisterModel model);

        Task<string> DeleteAsync(int accountId);
    }
}