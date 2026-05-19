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

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<AccessControlDashboardModel> GetDashboardCountsAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETDASHBOARDCOUNTS");

            return await conn.QueryFirstOrDefaultAsync<AccessControlDashboardModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AccessControlUserModel>> GetUsersAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETUSERS");

            return await conn.QueryAsync<AccessControlUserModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<AccessControlUserModel> GetUserDetailAsync(int accountId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETUSERDETAIL");
            parameters.Add("AccountId", accountId);

            return await conn.QueryFirstOrDefaultAsync<AccessControlUserModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AccessControlRoleModel>> GetRolesAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETROLES");

            return await conn.QueryAsync<AccessControlRoleModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateUserAsync(AccessControlUserModel model)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "UPDATEUSER");
            parameters.Add("AccountId", model.AccountId);
            parameters.Add("FullName", model.FullName);
            parameters.Add("Email", model.Email);
            parameters.Add("RoleId", model.RoleId);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AccessControlMenuModel>> GetMenusAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETALLMENUS");

            return await conn.QueryAsync<AccessControlMenuModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<int>> GetUserMenuIdsAsync(int accountId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETUSERMENUS");
            parameters.Add("AccountId", accountId);

            return await conn.QueryAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> SaveAccessAsync(int accountId, string menuIds)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "SAVEACCESS");
            parameters.Add("AccountId", accountId);
            parameters.Add("MenuIds", menuIds);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> RemoveAccessAsync(int accountId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "REMOVEACCESS");
            parameters.Add("AccountId", accountId);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AccessControlRoleModel>> GetRoleListAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETROLELIST");

            return await conn.QueryAsync<AccessControlRoleModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<AccessControlRoleModel> GetRoleByIdAsync(int roleId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETROLEBYID");
            parameters.Add("RoleId", roleId);

            return await conn.QueryFirstOrDefaultAsync<AccessControlRoleModel>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> SaveRoleAsync(string roleName)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "SAVEROLE");
            parameters.Add("RoleName", roleName);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateRoleAsync(AccessControlRoleModel model)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "UPDATEROLE");
            parameters.Add("RoleId", model.RoleId);
            parameters.Add("RoleName", model.RoleName);
            parameters.Add("IsActive", model.IsActive);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteRoleAsync(int roleId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "DELETEROLE");
            parameters.Add("RoleId", roleId);

            return await conn.ExecuteScalarAsync<int>("dbo.sp_AccessControl_Manage", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}