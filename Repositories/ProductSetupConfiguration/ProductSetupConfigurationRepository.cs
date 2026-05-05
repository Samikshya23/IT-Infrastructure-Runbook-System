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

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<ProductSetupConfiguration>> GetByProductIdAsync(int productId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETBYPRODUCT");
            parameters.Add("ProductId", productId);

            return await conn.QueryAsync<ProductSetupConfiguration>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ProductSetupConfiguration> GetJsonByProductIdAsync(int productId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETJSONBYPRODUCT");
            parameters.Add("ProductId", productId);

            return await conn.QueryFirstOrDefaultAsync<ProductSetupConfiguration>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> SaveOrUpdateJsonAsync(
            int productId,
            string setupJson,
            string createdBy)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "SAVEORUPDATEJSON");
            parameters.Add("ProductId", productId);
            parameters.Add("SetupJson", setupJson);
            parameters.Add("CreatedBy", createdBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteJsonByProductAsync(
            int productId,
            string deletedBy)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "DELETEJSONBYPRODUCT");
            parameters.Add("ProductId", productId);
            parameters.Add("DeletedBy", deletedBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}