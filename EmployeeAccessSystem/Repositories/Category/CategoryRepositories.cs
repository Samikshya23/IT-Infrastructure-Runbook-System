using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Repositories
{
    public class CategoryRepositories : ICategoryRepositories
    {
        private readonly string _connectionString;

        public CategoryRepositories(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Get all records
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<Category>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Get active records
        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETACTIVE");

                return await conn.QueryAsync<Category>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading active records.", ex);
            }
        }

        // Get record by id
        public async Task<Category> GetByIdAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETBYID");
                parameters.Add("CategoryId", id);

                return await conn.QueryFirstOrDefaultAsync<Category>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record.", ex);
            }
        }

        // Save new record
        public async Task<int> AddAsync(Category category)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "ADD");
                parameters.Add("Name", category.Name);
                parameters.Add("IsActive", category.IsActive);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Update existing record
        public async Task<int> UpdateAsync(Category category)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "UPDATE");
                parameters.Add("CategoryId", category.CategoryId);
                parameters.Add("Name", category.Name);
                parameters.Add("IsActive", category.IsActive);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating record.", ex);
            }
        }

        // Delete record
        public async Task<int> DeleteAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETE");
                parameters.Add("CategoryId", id);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }

        // Activate or deactivate record
        public async Task<int> ToggleAsync(int id)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "TOGGLE");
                parameters.Add("CategoryId", id);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_Category_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating status.", ex);
            }
        }
    }
}