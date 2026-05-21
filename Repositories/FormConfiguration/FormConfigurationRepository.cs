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
    public class FormConfigurationRepository : IFormConfigurationRepository
    {
        private readonly string _connectionString;

        public FormConfigurationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Load all active records
        public async Task<IEnumerable<FormConfiguration>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<FormConfiguration>("dbo.sp_FormConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Load configuration JSON by category
        public async Task<FormConfiguration> GetJsonByCategoryIdAsync(int categoryId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETJSONBYCATEGORY");
                parameters.Add("CategoryId", categoryId);

                return await conn.QueryFirstOrDefaultAsync<FormConfiguration>("dbo.sp_FormConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record.", ex);
            }
        }

        // Save or update configuration JSON
        public async Task<int> SaveOrUpdateJsonAsync(int categoryId, string configurationJson, string createdBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEORUPDATEJSON");
                parameters.Add("CategoryId", categoryId);
                parameters.Add("ConfigurationJson", configurationJson);
                parameters.Add("CreatedBy", createdBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_FormConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Delete configuration JSON by category
        public async Task<int> DeleteJsonByCategoryAsync(int categoryId, string deletedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEJSONBYCATEGORY");
                parameters.Add("CategoryId", categoryId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_FormConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }
    }
}