using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IAccountRepositories
    {
        Task<DbResult> RegisterAsync(RegisterModel model, byte[] passwordHash, byte[] passwordSalt, int createdBy);

        Task<Account?> GetByEmailAsync(string email);

        Task<Account?> GetByIdAsync(int accountId);

        Task<List<Account>> GetAllAsync();

        Task<DbResult> UpdateAsync(int accountId, RegisterModel model, int modifiedBy);

        Task<DbResult> DeleteAsync(int accountId, int deletedBy);

        Task<DbResult> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt);
    }
}
