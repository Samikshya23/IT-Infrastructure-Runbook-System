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
    public class ProductSetupConfigurationRepository : IProductSetupConfigurationRepository
    {
        private readonly string _connectionString;

        public ProductSetupConfigurationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Create database connection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Load all setup configurations
        public async Task<IEnumerable<ProductSetupConfiguration>> GetAllAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETALL");

                return await conn.QueryAsync<ProductSetupConfiguration>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading records.", ex);
            }
        }

        // Load configured products
        public async Task<IEnumerable<ProductSetupConfiguration>> GetConfiguredProductsAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETCONFIGUREDPRODUCTS");

                return await conn.QueryAsync<ProductSetupConfiguration>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading configured products.", ex);
            }
        }

        // Load setup by product
        public async Task<IEnumerable<ProductSetupConfiguration>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETBYPRODUCT");
                parameters.Add("ProductId", productId);

                return await conn.QueryAsync<ProductSetupConfiguration>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading record by product.", ex);
            }
        }

        // Load setup json by product
        public async Task<ProductSetupConfiguration> GetJsonByProductIdAsync(int productId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETJSONBYPRODUCT");
                parameters.Add("ProductId", productId);

                return await conn.QueryFirstOrDefaultAsync<ProductSetupConfiguration>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading setup json.", ex);
            }
        }

        // Save or update setup json
        public async Task<int> SaveOrUpdateJsonAsync(int productId, string setupJson, string createdBy, string modifiedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "SAVEORUPDATEJSON");
                parameters.Add("ProductId", productId);
                parameters.Add("SetupJson", setupJson);
                parameters.Add("CreatedBy", createdBy);
                parameters.Add("ModifiedBy", modifiedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving record.", ex);
            }
        }

        // Delete setup by product
        public async Task<int> DeleteJsonByProductAsync(int productId, string deletedBy)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "DELETEJSONBYPRODUCT");
                parameters.Add("ProductId", productId);
                parameters.Add("DeletedBy", deletedBy);

                return await conn.ExecuteScalarAsync<int>("dbo.sp_ProductSetupConfiguration_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting record.", ex);
            }
        }
    }
}