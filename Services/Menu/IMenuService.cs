using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IMenuService
    {
        Task<List<MenuModel>> GetSidebarMenusAsync(int accountId);
    }
}