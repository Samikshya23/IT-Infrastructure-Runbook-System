using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IAccessControlRepository
    {
        Task<AccessControlDashboardModel> GetDashboardCountsAsync();

        Task<IEnumerable<AccessControlUserModel>> GetUsersAsync();

        Task<AccessControlUserModel> GetUserDetailAsync(int accountId);

        Task<IEnumerable<AccessControlRoleModel>> GetRolesAsync();

        Task<int> UpdateUserAsync(AccessControlUserModel model);

        Task<IEnumerable<AccessControlMenuModel>> GetMenusAsync();

        Task<IEnumerable<int>> GetUserMenuIdsAsync(int accountId);

        Task<int> SaveAccessAsync(int accountId, string menuIds);

        Task<int> RemoveAccessAsync(int accountId);

        Task<IEnumerable<AccessControlRoleModel>> GetRoleListAsync();

        Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId);

        Task<int> SaveRoleAsync(string roleName);

        Task<int> UpdateRoleAsync(AccessControlRoleModel model);

        Task<int> DeleteRoleAsync(int roleId);
    }
}