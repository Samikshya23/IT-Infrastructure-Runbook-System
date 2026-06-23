using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Helpers;
using EmployeeAccessSystem.Repositories;
using EmployeeAccessSystem.Services;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Services
{
    public class AccessControlService : IAccessControlService
    {
        private readonly IAccessControlRepository _repository;
        private readonly IAccountRepositories _accountRepo;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AccessControlService(
            IAccessControlRepository repository,
            IAccountRepositories accountRepo,
            IConfiguration config,
            IEmailService emailService)
        {
            _repository = repository;
            _accountRepo = accountRepo;
            _config = config;
            _emailService = emailService;
        }

        // Dashboard
        public async Task<AccessControlDashboardModel> GetDashboardAsync()
        {
            AccessControlDashboardModel model = await _repository.GetDashboardCountsAsync();

            if (model == null)
            {
                model = new AccessControlDashboardModel();
            }

            return model;
        }

        // Users
        public async Task<List<AccessControlUserModel>> GetUsersAsync()
        {
            IEnumerable<AccessControlUserModel> users = await _repository.GetUsersAsync();

            if (users == null)
            {
                return new List<AccessControlUserModel>();
            }

            return users.ToList();
        }

        public async Task<AccessControlUserModel> GetUserDetailAsync(int accountId)
        {
            if (accountId <= 0)
            {
                return new AccessControlUserModel();
            }

            AccessControlUserModel model = await _repository.GetUserDetailAsync(accountId);

            if (model == null)
            {
                model = new AccessControlUserModel();
            }

            return model;
        }

        public async Task<string> UpdateUserAsync(AccessControlUserModel model, int currentUserId)
        {
            if (model == null)
            {
                return "Invalid user information.";
            }

            if (model.AccountId <= 0)
            {
                return "Please select a valid user.";
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
                return "Please select a role.";
            }

            int result = await _repository.UpdateUserAsync(model, currentUserId);

            if (result > 0)
            {
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        return "Password and confirm password do not match.";
                    }

                    string passwordKey = _config["Security:PasswordKey"] ?? "";

                    Helpers.Helper.CreatePasswordHash(
                        model.Password,
                        passwordKey,
                        out byte[] passwordHash,
                        out byte[] passwordSalt
                    );

                    var pwdResult = await _accountRepo.UpdatePasswordAsync(model.Email, passwordHash, passwordSalt);
                    if (pwdResult.ResultId > 0)
                    {
                        try
                        {
                            await _emailService.SendForgotPasswordEmailAsync(model.Email, model.FullName, model.Password);
                        }
                        catch (Exception)
                        {
                            // Ignore email errors to avoid blocking successful user updates
                        }
                    }
                }

                return "User updated successfully.";
            }

            return "Unable to update user.";
        }

        public async Task<string> DeleteUserAsync(int accountId, int currentUserId)
        {
            if (accountId <= 0)
            {
                return "Please select a valid user.";
            }

            if (accountId == currentUserId)
            {
                return "You cannot delete your own account.";
            }

            int result = await _repository.DeleteUserAsync(accountId, currentUserId);

            if (result == 1)
            {
                return "User deleted successfully.";
            }

            if (result == -1)
            {
                return "Administrator user cannot be deleted.";
            }

            if (result == -2)
            {
                return "You cannot delete your own account.";
            }

            if (result == -3)
            {
                return "User not found.";
            }

            return "Unable to delete user.";
        }

        // Roles
        public async Task<List<AccessControlRoleModel>> GetRolesAsync()
        {
            IEnumerable<AccessControlRoleModel> roles = await _repository.GetRolesAsync();

            if (roles == null)
            {
                return new List<AccessControlRoleModel>();
            }

            return roles.ToList();
        }

        public async Task<List<AccessControlRoleModel>> GetRoleListAsync()
        {
            IEnumerable<AccessControlRoleModel> roles = await _repository.GetRoleListAsync();

            if (roles == null)
            {
                return new List<AccessControlRoleModel>();
            }

            return roles.ToList();
        }

        public async Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId)
        {
            if (roleId <= 0)
            {
                return new AccessControlRoleModel();
            }

            AccessControlRoleModel role = await _repository.GetRoleByIdAsync(roleId);

            if (role == null)
            {
                role = new AccessControlRoleModel();
            }

            return role;
        }

        public async Task<string> SaveRoleAsync(string roleName, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return "Role name is required.";
            }

            int result = await _repository.SaveRoleAsync(roleName.Trim(), currentUserId);

            if (result == 1)
            {
                return "Role saved successfully.";
            }

            if (result == -1)
            {
                return "Role already exists.";
            }

            return "Unable to save role.";
        }

        public async Task<string> UpdateRoleAsync(AccessControlRoleModel model, int currentUserId)
        {
            if (model == null)
            {
                return "Invalid role information.";
            }

            if (model.RoleId <= 0)
            {
                return "Please select a valid role.";
            }

            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                return "Role name is required.";
            }

            int result = await _repository.UpdateRoleAsync(model, currentUserId);

            if (result == 1)
            {
                return "Role updated successfully.";
            }

            if (result == -1)
            {
                return "Role already exists.";
            }

            if (result == -2)
            {
                return "Administrator role cannot be modified.";
            }

            return "Unable to update role.";
        }

        public async Task<string> DeleteRoleAsync(int roleId, int currentUserId)
        {
            if (roleId <= 0)
            {
                return "Please select a valid role.";
            }

            int result = await _repository.DeleteRoleAsync(roleId, currentUserId);

            if (result == 1)
            {
                return "Role deleted successfully.";
            }

            if (result == -1)
            {
                return "Administrator role cannot be deleted.";
            }

            if (result == -2)
            {
                return "This role is assigned to users and cannot be deleted.";
            }

            return "Unable to delete role.";
        }

        // User permission
        public async Task<List<AccessControlRoleActionModel>> GetUserPermissionItemsAsync(int accountId)
        {
            IEnumerable<AccessControlRoleActionModel> items = await _repository.GetPermissionItemsAsync();
            IEnumerable<int> selectedIds = await _repository.GetUserSelectedAsync(accountId);

            List<AccessControlRoleActionModel> permissionItems = new List<AccessControlRoleActionModel>();
            List<int> selectedMenuIds = new List<int>();

            if (items != null)
            {
                permissionItems = items.ToList();
            }

            if (selectedIds != null)
            {
                selectedMenuIds = selectedIds.ToList();
            }

            foreach (AccessControlRoleActionModel item in permissionItems)
            {
                item.IsChecked = selectedMenuIds.Contains(item.MenuId);
            }

            return permissionItems;
        }

        public async Task<string> SaveUserAccessAsync(int accountId, List<int> checkedMenuIds, List<int> uncheckedMenuIds, int currentUserId)
        {
            if (accountId <= 0)
            {
                return "Please select a valid user.";
            }

            if (checkedMenuIds == null)
            {
                checkedMenuIds = new List<int>();
            }

            if (uncheckedMenuIds == null)
            {
                uncheckedMenuIds = new List<int>();
            }

            string checkedIds = string.Join(",", checkedMenuIds);
            string uncheckedIds = string.Join(",", uncheckedMenuIds);

            int result = await _repository.SaveUserAccessAsync(accountId, checkedIds, uncheckedIds, currentUserId);

            if (result >= 0)
            {
                return "Permission saved successfully.";
            }

            return "Unable to save permission.";
        }

        public async Task<string> ClearUserAccessAsync(int accountId, int currentUserId)
        {
            if (accountId <= 0)
            {
                return "Please select a valid user.";
            }

            int result = await _repository.ClearUserAccessAsync(accountId, currentUserId);

            if (result >= 0)
            {
                return "User permission cleared successfully.";
            }

            return "Unable to clear user permission.";
        }

        // Role permission
        public async Task<List<AccessControlRoleActionModel>> GetRolePermissionItemsAsync(int roleId)
        {
            IEnumerable<AccessControlRoleActionModel> items = await _repository.GetPermissionItemsAsync();
            IEnumerable<int> selectedIds = await _repository.GetRoleSelectedAsync(roleId);

            List<AccessControlRoleActionModel> permissionItems = new List<AccessControlRoleActionModel>();
            List<int> selectedMenuIds = new List<int>();

            if (items != null)
            {
                permissionItems = items.ToList();
            }

            if (selectedIds != null)
            {
                selectedMenuIds = selectedIds.ToList();
            }

            foreach (AccessControlRoleActionModel item in permissionItems)
            {
                item.IsChecked = selectedMenuIds.Contains(item.MenuId);
            }

            return permissionItems;
        }

        public async Task<string> SaveRoleAccessAsync(int roleId, List<int> checkedMenuIds, int currentUserId)
        {
            if (roleId <= 0)
            {
                return "Please select a valid role.";
            }

            if (checkedMenuIds == null)
            {
                checkedMenuIds = new List<int>();
            }

            string checkedIds = string.Join(",", checkedMenuIds);

            int result = await _repository.SaveRoleAccessAsync(roleId, checkedIds, currentUserId);

            if (result >= 0)
            {
                return "Permission saved successfully.";
            }

            return "Unable to save permission.";
        }

        public async Task<string> ClearRoleAccessAsync(int roleId, int currentUserId)
        {
            if (roleId <= 0)
            {
                return "Please select a valid role.";
            }

            int result = await _repository.ClearRoleAccessAsync(roleId, currentUserId);

            if (result >= 0)
            {
                return "Role permission cleared successfully.";
            }

            return "Unable to clear role permission.";
        }
    }
}