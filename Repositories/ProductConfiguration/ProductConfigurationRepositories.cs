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

        public async Task<IEnumerable<ProductConfiguration>> GetAllAsync()
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETALL");

            return await conn.QueryAsync<ProductConfiguration>(
                "dbo.sp_ProductConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ProductConfiguration> GetJsonByProductIdAsync(int productId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETJSONBYPRODUCT");
            parameters.Add("ProductId", productId);

            return await conn.QueryFirstOrDefaultAsync<ProductConfiguration>(
                "dbo.sp_ProductConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> SaveOrUpdateJsonAsync(int productId, string configurationJson, string createdBy)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "SAVEORUPDATEJSON");
            parameters.Add("ProductId", productId);
            parameters.Add("ConfigurationJson", configurationJson);
            parameters.Add("CreatedBy", createdBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteJsonByProductAsync(int productId, string deletedBy)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "DELETEJSONBYPRODUCT");
            parameters.Add("ProductId", productId);
            parameters.Add("DeletedBy", deletedBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}