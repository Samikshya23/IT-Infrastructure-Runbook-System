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
    public class CategorySetupRepository : ICategorySetupRepository
    {
        private readonly string _connectionString;

        public CategorySetupRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Create database connection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Load all setup configurations
        public async Task<IEnumerable<CategorySetup>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<CategorySetup>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Load configured categories
        public async Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETCONFIGUREDCATEGORIES");

                return await conn.QueryAsync<CategorySetup>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading configured categories.", ex);
            }
        }

        // Load setup by category
        public async Task<IEnumerable<CategorySetup>> GetByCategoryIdAsync(int categoryId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETBYCATEGORY");
                parameters.Add("CategoryId", categoryId);

                return await conn.QueryAsync<CategorySetup>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record by category.", ex);
            }
        }

        // Load setup json by category
        public async Task<CategorySetup> GetJsonByCategoryIdAsync(int categoryId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETJSONBYCATEGORY");
                parameters.Add("CategoryId", categoryId);

                return await conn.QueryFirstOrDefaultAsync<CategorySetup>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading setup json.", ex);
            }
        }

        // Save or update setup json
        public async Task<int> SaveOrUpdateJsonAsync(int categoryId, string setupJson, string createdBy, string modifiedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEORUPDATEJSON");
                parameters.Add("CategoryId", categoryId);
                parameters.Add("SetupJson", setupJson);
                parameters.Add("CreatedBy", createdBy);
                parameters.Add("ModifiedBy", modifiedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Delete setup by category
        public async Task<int> DeleteJsonByCategoryAsync(int categoryId, string deletedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEJSONBYCATEGORY");
                parameters.Add("CategoryId", categoryId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_CategorySetup_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }
    }
}