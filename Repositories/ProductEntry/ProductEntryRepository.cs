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
    public class ProductEntryRepository : IProductEntryRepository
    {
        private readonly IConfiguration _configuration;

        public ProductEntryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection Connection
        {
            get
            {
                return new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection"));
            }
        }

        public async Task<IEnumerable<ProductEntryModel>> GetAllAsync()
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "GETALL");

            using IDbConnection db = Connection;

            IEnumerable<ProductEntryModel> data =
                await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return data;
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "GETBYPRODUCT");
            parameters.Add("@ProductId", productId);

            using IDbConnection db = Connection;

            IEnumerable<ProductEntryModel> data =
                await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return data;
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByGroupAsync(Guid entryGroupId)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "GETBYGROUP");
            parameters.Add("@EntryGroupId", entryGroupId);

            using IDbConnection db = Connection;

            IEnumerable<ProductEntryModel> data =
                await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return data;
        }

        public async Task<string> GetSetupJsonByProductAsync(int productId)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "GETSETUPJSON");
            parameters.Add("@ProductId", productId);

            using IDbConnection db = Connection;

            string setupJson =
                await db.QueryFirstOrDefaultAsync<string>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return setupJson;
        }

        public async Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync()
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "GETCONFIGUREDPRODUCTS");

            using IDbConnection db = Connection;

            IEnumerable<ProductConfigurationIndexItem> products =
                await db.QueryAsync<ProductConfigurationIndexItem>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return products;
        }

        public async Task<int> CheckExistsAsync(int productId, DateTime entryDate)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "CHECK_EXISTS");
            parameters.Add("@ProductId", productId);
            parameters.Add("@EntryDate", entryDate);

            using IDbConnection db = Connection;

            int count =
                await db.ExecuteScalarAsync<int>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);

            return count;
        }

        public async Task AddAsync(ProductEntryModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "INSERT");
            parameters.Add("@EntryGroupId", model.EntryGroupId);
            parameters.Add("@ProductId", model.ProductId);
            parameters.Add("@SetupNodeId", model.SetupNodeId);
            parameters.Add("@ParentPath", model.ParentPath);
            parameters.Add("@DisplayName", model.DisplayName);
            parameters.Add("@ValueType", model.ValueType);
            parameters.Add("@ResultValue", model.ResultValue);
            parameters.Add("@EntryDate", model.EntryDate);
            parameters.Add("@IsActive", model.IsActive);
            parameters.Add("@CreatedBy", model.CreatedBy);

            using IDbConnection db = Connection;

            await db.ExecuteAsync(
                "sp_ProductEntry_Manage",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task DeleteGroupAsync(Guid entryGroupId, string deletedBy)
        {
            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@Flag", "DELETEGROUP");
            parameters.Add("@EntryGroupId", entryGroupId);
            parameters.Add("@DeletedBy", deletedBy);

            using IDbConnection db = Connection;

            await db.ExecuteAsync(
                "sp_ProductEntry_Manage",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}