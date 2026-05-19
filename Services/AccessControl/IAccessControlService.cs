using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IAccessControlService
    {
        Task<AccessControlDashboardModel> GetDashboardAsync();

        Task<List<AccessControlUserModel>> GetUsersAsync();

        Task<AccessControlUserModel> GetUserDetailAsync(int accountId);

        Task<List<AccessControlRoleModel>> GetRolesAsync();

        Task<string> UpdateUserAsync(AccessControlUserModel model);

        Task<List<AccessControlMenuModel>> GetMenusByUserAsync(int accountId);

        Task<string> SaveAccessAsync(int accountId, List<int> menuIds);

        Task<string> RemoveAccessAsync(int accountId);

        Task<List<AccessControlRoleModel>> GetRoleListAsync();

        Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId);

        Task<string> SaveRoleAsync(string roleName);

        Task<string> UpdateRoleAsync(AccessControlRoleModel model);

        Task<string> DeleteRoleAsync(int roleId);
    }
}