using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Repositories
{
    public class AccountRepositories : IAccountRepositories
    {
        private readonly string _connectionString;

        public AccountRepositories(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<DbResult> RegisterAsync(RegisterModel model, byte[] passwordHash, byte[] passwordSalt, int createdBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "REGISTER");
                parameters.Add("FullName", model.FullName);
                parameters.Add("Email", model.Email);
                parameters.Add("PasswordHash", passwordHash);
                parameters.Add("PasswordSalt", passwordSalt);
                parameters.Add("RoleId", model.RoleId);
                parameters.Add("CreatedBy", createdBy);

                DbResult result = await conn.QueryFirstOrDefaultAsync<DbResult>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    return new DbResult
                    {
                        ResultId = 0,
                        Message = "Something went wrong while registering user."
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while registering user.", ex);
            }
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "GETBYEMAIL");
                parameters.Add("Email", email);

                Account? account = await conn.QueryFirstOrDefaultAsync<Account>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return account;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading account by email.", ex);
            }
        }

        public async Task<Account?> GetByIdAsync(int accountId)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "GETBYID");
                parameters.Add("AccountId", accountId);

                Account? account = await conn.QueryFirstOrDefaultAsync<Account>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return account;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading account detail.", ex);
            }
        }

        public async Task<List<Account>> GetAllAsync()
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "GETALL");

                IEnumerable<Account> accounts = await conn.QueryAsync<Account>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return accounts.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading accounts.", ex);
            }
        }

        public async Task<DbResult> UpdateAsync(int accountId, RegisterModel model, int modifiedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "UPDATE");
                parameters.Add("AccountId", accountId);
                parameters.Add("FullName", model.FullName);
                parameters.Add("Email", model.Email);
                parameters.Add("RoleId", model.RoleId);
                parameters.Add("ModifiedBy", modifiedBy);

                DbResult result = await conn.QueryFirstOrDefaultAsync<DbResult>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    return new DbResult
                    {
                        ResultId = 0,
                        Message = "Something went wrong while updating user."
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating user.", ex);
            }
        }

        public async Task<DbResult> DeleteAsync(int accountId, int deletedBy)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "DELETE");
                parameters.Add("AccountId", accountId);
                parameters.Add("DeletedBy", deletedBy);

                DbResult result = await conn.QueryFirstOrDefaultAsync<DbResult>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    return new DbResult
                    {
                        ResultId = 0,
                        Message = "Something went wrong while deleting user."
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting user.", ex);
            }
        }

        public async Task<DbResult> UpdatePasswordAsync(string email, byte[] passwordHash, byte[] passwordSalt)
        {
            try
            {
                using var conn = CreateConnection();

                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("Flag", "UPDATEPASSWORD");
                parameters.Add("Email", email);
                parameters.Add("PasswordHash", passwordHash);
                parameters.Add("PasswordSalt", passwordSalt);

                DbResult result = await conn.QueryFirstOrDefaultAsync<DbResult>(
                    "dbo.sp_Account_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                {
                    return new DbResult
                    {
                        ResultId = 0,
                        Message = "Something went wrong while updating password."
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating user password.", ex);
            }
        }
    }
}
