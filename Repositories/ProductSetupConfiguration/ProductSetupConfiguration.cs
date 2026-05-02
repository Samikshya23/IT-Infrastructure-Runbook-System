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

        public async Task<ProductSetupConfiguration> GetNodeByIdAsync(int nodeId)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "GETNODEBYID");
            parameters.Add("NodeId", nodeId);

            return await conn.QueryFirstOrDefaultAsync<ProductSetupConfiguration>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> CheckDuplicateNodeAsync(int productId, int configurationNodeId, int? parentNodeId, string nodeValue)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "CHECKDUPLICATE");
            parameters.Add("ProductId", productId);
            parameters.Add("ConfigurationNodeId", configurationNodeId);
            parameters.Add("ParentNodeId", parentNodeId);
            parameters.Add("NodeValue", nodeValue);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> AddAsync(ProductSetupConfiguration model)
        {
            using var conn = GetConnection();

            int? sortOrderValue = null;

            if (model.SortOrder > 0)
            {
                sortOrderValue = model.SortOrder;
            }

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "ADD");
            parameters.Add("ProductId", model.ProductId);
            parameters.Add("ConfigurationNodeId", model.ConfigurationNodeId);
            parameters.Add("ParentNodeId", model.ParentNodeId);
            parameters.Add("NodeValue", model.NodeValue);
            parameters.Add("InputType", model.InputType);
            parameters.Add("SortOrder", sortOrderValue);
            parameters.Add("CreatedBy", model.CreatedBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> UpdateNodeAsync(ProductSetupConfiguration model)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "UPDATENODE");
            parameters.Add("NodeId", model.NodeId);
            parameters.Add("NodeValue", model.NodeValue);
            parameters.Add("ModifiedBy", model.ModifiedBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteNodeAsync(int nodeId, string deletedBy)
        {
            using var conn = GetConnection();

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("Flag", "DELETENODE");
            parameters.Add("NodeId", nodeId);
            parameters.Add("DeletedBy", deletedBy);

            return await conn.ExecuteScalarAsync<int>(
                "dbo.sp_ProductSetupConfiguration_Manage",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}