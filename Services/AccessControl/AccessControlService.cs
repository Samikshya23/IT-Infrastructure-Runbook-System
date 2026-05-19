using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class AccessControlService : IAccessControlService
    {
        private readonly IAccessControlRepository _repository;

        public AccessControlService(IAccessControlRepository repository)
        {
            _repository = repository;
        }
        public async Task<AccessControlDashboardModel> GetDashboardAsync()
        {
            AccessControlDashboardModel dashboard = await _repository.GetDashboardCountsAsync();

            if (dashboard == null)
            {
                dashboard = new AccessControlDashboardModel();
            }

            dashboard.Users = await GetUsersAsync();

            return dashboard;
        }
        public async Task<List<AccessControlUserModel>> GetUsersAsync()
        {
            IEnumerable<AccessControlUserModel> result = await _repository.GetUsersAsync();

            List<AccessControlUserModel> users = new List<AccessControlUserModel>();

            foreach (AccessControlUserModel item in result)
            {
                users.Add(item);
            }

            return users;
        }
        public async Task<AccessControlUserModel> GetUserDetailAsync(int accountId)
        {
            return await _repository.GetUserDetailAsync(accountId);
        }
        public async Task<List<AccessControlRoleModel>> GetRolesAsync()
        {
            IEnumerable<AccessControlRoleModel> result = await _repository.GetRolesAsync();

            List<AccessControlRoleModel> roles = new List<AccessControlRoleModel>();

            foreach (AccessControlRoleModel item in result)
            {
                roles.Add(item);
            }

            return roles;
        }
        public async Task<string> UpdateUserAsync(AccessControlUserModel model)
        {
            if (model.AccountId <= 0)
            {
                return "Invalid user selected.";
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return "Full name is required.";
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return "Email is required.";
            }

            if (model.RoleId <= 0)
            {
                return "Please select role.";
            }

            await _repository.UpdateUserAsync(model);

            return "";
        }

        public async Task<List<AccessControlMenuModel>> GetMenusByUserAsync(int accountId)
        {
            AccessControlUserModel user = await _repository.GetUserDetailAsync(accountId);

            IEnumerable<AccessControlMenuModel> menuResult = await _repository.GetMenusAsync();
            IEnumerable<int> selectedResult = await _repository.GetUserMenuIdsAsync(accountId);

            List<int> selectedIds = new List<int>();

            foreach (int id in selectedResult)
            {
                selectedIds.Add(id);
            }

            List<AccessControlMenuModel> menuList = new List<AccessControlMenuModel>();

            foreach (AccessControlMenuModel menu in menuResult)
            {
                if (user != null && user.RoleName == "Administrator")
                {
                    menu.IsChecked = true;
                }
                else
                {
                    foreach (int selectedId in selectedIds)
                    {
                        if (menu.MenuId == selectedId)
                        {
                            menu.IsChecked = true;
                            break;
                        }
                    }
                }

                menuList.Add(menu);
            }

            List<AccessControlMenuModel> parents = new List<AccessControlMenuModel>();

            foreach (AccessControlMenuModel menu in menuList)
            {
                if (menu.ParentMenuId == null)
                {
                    parents.Add(menu);
                }
            }

            foreach (AccessControlMenuModel parent in parents)
            {
                foreach (AccessControlMenuModel child in menuList)
                {
                    if (child.ParentMenuId == parent.MenuId)
                    {
                        parent.Children.Add(child);
                    }
                }
            }

            return parents;
        }

        public async Task<string> SaveAccessAsync(int accountId, List<int> menuIds)
        {
            if (accountId <= 0)
            {
                return "Invalid user selected.";
            }

            AccessControlUserModel user = await _repository.GetUserDetailAsync(accountId);

            if (user != null && user.RoleName == "Administrator")
            {
                return "Administrator always has full access. No need to assign menu access.";
            }

            string ids = "";

            if (menuIds != null && menuIds.Count > 0)
            {
                ids = string.Join(",", menuIds);
            }

            await _repository.SaveAccessAsync(accountId, ids);

            return "";
        }

        public async Task<string> RemoveAccessAsync(int accountId)
        {
            if (accountId <= 0)
            {
                return "Invalid user selected.";
            }

            AccessControlUserModel user = await _repository.GetUserDetailAsync(accountId);

            if (user != null && user.RoleName == "Administrator")
            {
                return "Administrator access cannot be removed.";
            }

            await _repository.RemoveAccessAsync(accountId);

            return "";
        }

        public async Task<List<AccessControlRoleModel>> GetRoleListAsync()
        {
            IEnumerable<AccessControlRoleModel> result = await _repository.GetRoleListAsync();

            List<AccessControlRoleModel> roles = new List<AccessControlRoleModel>();

            foreach (AccessControlRoleModel item in result)
            {
                roles.Add(item);
            }

            return roles;
        }

        public async Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId)
        {
            return await _repository.GetRoleByIdAsync(roleId);
        }

        public async Task<string> SaveRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return "Role name is required.";
            }

            await _repository.SaveRoleAsync(roleName);

            return "";
        }

        public async Task<string> UpdateRoleAsync(AccessControlRoleModel model)
        {
            if (model.RoleId <= 0)
            {
                return "Invalid role selected.";
            }

            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                return "Role name is required.";
            }

            await _repository.UpdateRoleAsync(model);

            return "";
        }

        public async Task<string> DeleteRoleAsync(int roleId)
        {
            if (roleId <= 0)
            {
                return "Invalid role selected.";
            }

            int result = await _repository.DeleteRoleAsync(roleId);

            if (result == -1)
            {
                return "This role is assigned to a user. Please change that user role first.";
            }

            return "";
        }
    }
}