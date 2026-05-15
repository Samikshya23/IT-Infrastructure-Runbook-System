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
    public class ProductConfigurationRepository : IProductConfigurationRepository
    {
        private readonly string _connectionString;

        public ProductConfigurationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        // Load all active records
        public async Task<IEnumerable<ProductConfiguration>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<ProductConfiguration>("dbo.sp_ProductConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Load configuration JSON by product
        public async Task<ProductConfiguration> GetJsonByProductIdAsync(int productId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETJSONBYPRODUCT");
                parameters.Add("ProductId", productId);

                return await conn.QueryFirstOrDefaultAsync<ProductConfiguration>("dbo.sp_ProductConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record.", ex);
            }
        }

        // Save or update configuration JSON
        public async Task<int> SaveOrUpdateJsonAsync(int productId, string configurationJson, string createdBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEORUPDATEJSON");
                parameters.Add("ProductId", productId);
                parameters.Add("ConfigurationJson", configurationJson);
                parameters.Add("CreatedBy", createdBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Delete configuration JSON by product
        public async Task<int> DeleteJsonByProductAsync(int productId, string deletedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEJSONBYPRODUCT");
                parameters.Add("ProductId", productId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }
    }
}