using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuModel>> GetMenusByAccountIdAsync(int accountId);
    }
}