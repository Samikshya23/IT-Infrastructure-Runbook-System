using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IAccessControlService
    {
        // Dashboard
        Task<AccessControlDashboardModel> GetDashboardAsync();

        // Users
        Task<List<AccessControlUserModel>> GetUsersAsync();
        Task<AccessControlUserModel> GetUserDetailAsync(int accountId);
        Task<string> UpdateUserAsync(AccessControlUserModel model, int currentUserId);
        Task<string> DeleteUserAsync(int accountId, int currentUserId);

        // Roles
        Task<List<AccessControlRoleModel>> GetRolesAsync();
        Task<List<AccessControlRoleModel>> GetRoleListAsync();
        Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId);
        Task<string> SaveRoleAsync(string roleName, int currentUserId);
        Task<string> UpdateRoleAsync(AccessControlRoleModel model, int currentUserId);
        Task<string> DeleteRoleAsync(int roleId, int currentUserId);

        // User permission
        Task<List<AccessControlRoleActionModel>> GetUserPermissionItemsAsync(int accountId);
        Task<string> SaveUserAccessAsync(int accountId, List<int> checkedMenuIds, List<int> uncheckedMenuIds, int currentUserId);
        Task<string> ClearUserAccessAsync(int accountId, int currentUserId);

        // Role permission
        Task<List<AccessControlRoleActionModel>> GetRolePermissionItemsAsync(int roleId);
        Task<string> SaveRoleAccessAsync(int roleId, List<int> checkedMenuIds, int currentUserId);
        Task<string> ClearRoleAccessAsync(int roleId, int currentUserId);
    }
}