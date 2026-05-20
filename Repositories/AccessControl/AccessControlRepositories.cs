using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Repositories
{
    public class AccessControlRepository : IAccessControlRepository
    {
        private readonly string _connectionString;

        public AccessControlRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Dashboard
        public async Task<AccessControlDashboardModel> GetDashboardCountsAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DASHBOARD");

                return await conn.QueryFirstOrDefaultAsync<AccessControlDashboardModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading dashboard counts.", ex);
            }
        }

        // Users
        public async Task<IEnumerable<AccessControlUserModel>> GetUsersAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETUSERS");

                return await conn.QueryAsync<AccessControlUserModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading users.", ex);
            }
        }

        public async Task<AccessControlUserModel> GetUserDetailAsync(int accountId)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETUSERDETAIL");
                parameters.Add("AccountId", accountId);

                return await conn.QueryFirstOrDefaultAsync<AccessControlUserModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading user detail.", ex);
            }
        }

        public async Task<int> UpdateUserAsync(AccessControlUserModel model, int modifiedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "UPDATEUSER");
                parameters.Add("AccountId", model.AccountId);
                parameters.Add("FullName", model.FullName);
                parameters.Add("Email", model.Email);
                parameters.Add("RoleId", model.RoleId);
                parameters.Add("IsActive", model.IsActive);
                parameters.Add("ModifiedBy", modifiedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating user.", ex);
            }
        }

        public async Task<int> DeleteUserAsync(int accountId, int deletedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEUSER");
                parameters.Add("AccountId", accountId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting user.", ex);
            }
        }

        // Roles
        public async Task<IEnumerable<AccessControlRoleModel>> GetRolesAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETROLES");

                return await conn.QueryAsync<AccessControlRoleModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading roles.", ex);
            }
        }

        public async Task<IEnumerable<AccessControlRoleModel>> GetRoleListAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETROLELIST");

                return await conn.QueryAsync<AccessControlRoleModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading role list.", ex);
            }
        }

        public async Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETROLEBYID");
                parameters.Add("RoleId", roleId);

                return await conn.QueryFirstOrDefaultAsync<AccessControlRoleModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading role detail.", ex);
            }
        }

        public async Task<int> SaveRoleAsync(string roleName, int createdBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEROLE");
                parameters.Add("RoleName", roleName);
                parameters.Add("CreatedBy", createdBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving role.", ex);
            }
        }

        public async Task<int> UpdateRoleAsync(AccessControlRoleModel model, int modifiedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "UPDATEROLE");
                parameters.Add("RoleId", model.RoleId);
                parameters.Add("RoleName", model.RoleName);
                parameters.Add("IsActive", model.IsActive);
                parameters.Add("ModifiedBy", modifiedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating role.", ex);
            }
        }

        public async Task<int> DeleteRoleAsync(int roleId, int deletedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEROLE");
                parameters.Add("RoleId", roleId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting role.", ex);
            }
        }

        // Permission items
        public async Task<IEnumerable<AccessControlRoleActionModel>> GetPermissionItemsAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETPERMISSIONITEMS");

                return await conn.QueryAsync<AccessControlRoleActionModel>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading permission items.", ex);
            }
        }

        // User permission
        public async Task<IEnumerable<int>> GetUserSelectedAsync(int accountId)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETUSERSELECTED");
                parameters.Add("AccountId", accountId);

                return await conn.QueryAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading selected user permission.", ex);
            }
        }

        public async Task<int> SaveUserAccessAsync(int accountId, string checkedMenuIds, string uncheckedMenuIds, int createdBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEUSERACCESS");
                parameters.Add("AccountId", accountId);
                parameters.Add("CheckedMenuIds", checkedMenuIds);
                parameters.Add("UncheckedMenuIds", uncheckedMenuIds);
                parameters.Add("CreatedBy", createdBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving user permission.", ex);
            }
        }

        public async Task<int> ClearUserAccessAsync(int accountId, int deletedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "CLEARUSERACCESS");
                parameters.Add("AccountId", accountId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while clearing user permission.", ex);
            }
        }

        // Role permission
        public async Task<IEnumerable<int>> GetRoleSelectedAsync(int roleId)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETROLESELECTED");
                parameters.Add("RoleId", roleId);

                return await conn.QueryAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading selected role permission.", ex);
            }
        }

        public async Task<int> SaveRoleAccessAsync(int roleId, string checkedMenuIds, int createdBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEROLEACCESS");
                parameters.Add("RoleId", roleId);
                parameters.Add("CheckedMenuIds", checkedMenuIds);
                parameters.Add("CreatedBy", createdBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving role permission.", ex);
            }
        }

        public async Task<int> ClearRoleAccessAsync(int roleId, int deletedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "CLEARROLEACCESS");
                parameters.Add("RoleId", roleId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>(
                    "dbo.sp_AccessControl_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while clearing role permission.", ex);
            }
        }
    }
}