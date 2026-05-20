using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IAccessControlRepository
    {
        // Dashboard
        Task<AccessControlDashboardModel> GetDashboardCountsAsync();

        // Users
        Task<IEnumerable<AccessControlUserModel>> GetUsersAsync();
        Task<AccessControlUserModel> GetUserDetailAsync(int accountId);
        Task<int> UpdateUserAsync(AccessControlUserModel model, int modifiedBy);
        Task<int> DeleteUserAsync(int accountId, int deletedBy);

        // Roles
        Task<IEnumerable<AccessControlRoleModel>> GetRolesAsync();
        Task<IEnumerable<AccessControlRoleModel>> GetRoleListAsync();
        Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId);
        Task<int> SaveRoleAsync(string roleName, int createdBy);
        Task<int> UpdateRoleAsync(AccessControlRoleModel model, int modifiedBy);
        Task<int> DeleteRoleAsync(int roleId, int deletedBy);

        // Permission items
        Task<IEnumerable<AccessControlRoleActionModel>> GetPermissionItemsAsync();

        // User permission
        Task<IEnumerable<int>> GetUserSelectedAsync(int accountId);
        Task<int> SaveUserAccessAsync(int accountId, string checkedMenuIds, string uncheckedMenuIds, int createdBy);
        Task<int> ClearUserAccessAsync(int accountId, int deletedBy);

        // Role permission
        Task<IEnumerable<int>> GetRoleSelectedAsync(int roleId);
        Task<int> SaveRoleAccessAsync(int roleId, string checkedMenuIds, int createdBy);
        Task<int> ClearRoleAccessAsync(int roleId, int deletedBy);
    }
}