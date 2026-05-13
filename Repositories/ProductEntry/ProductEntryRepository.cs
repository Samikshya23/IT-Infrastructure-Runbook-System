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
                return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            }
        }

        public async Task<IEnumerable<ProductEntryModel>> GetAllAsync()
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETALL");

                using IDbConnection db = Connection;

                return await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load product entries.", ex);
            }
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETBYPRODUCT");
                parameters.Add("@ProductId", productId);

                using IDbConnection db = Connection;

                return await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load product entries by product.", ex);
            }
        }

        public async Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETDETAILS");
                parameters.Add("@EntryGroupId", entryGroupId);

                using IDbConnection db = Connection;

                return await db.QueryAsync<ProductEntryModel>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load product entry details.", ex);
            }
        }

        public async Task<string> GetSetupAsync(int productId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETSETUP");
                parameters.Add("@ProductId", productId);

                using IDbConnection db = Connection;

                return await db.QueryFirstOrDefaultAsync<string>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load product setup.", ex);
            }
        }

        public async Task<string> GetConfigurationAsync(int productId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETCONFIGURATION");
                parameters.Add("@ProductId", productId);

                using IDbConnection db = Connection;

                return await db.QueryFirstOrDefaultAsync<string>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load product configuration.", ex);
            }
        }

        public async Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync()
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETCONFIGUREDPRODUCTS");

                using IDbConnection db = Connection;

                return await db.QueryAsync<ProductConfigurationIndexItem>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load configured products.", ex);
            }
        }

        public async Task<int> CheckExistsAsync(int productId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "CHECKEXISTS");
                parameters.Add("@ProductId", productId);

                using IDbConnection db = Connection;

                return await db.ExecuteScalarAsync<int>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check product entry.", ex);
            }
        }

        public async Task<int> SaveAsync(ProductEntryModel model)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@Flag", "SAVE");
                parameters.Add("@EntryGroupId", model.EntryGroupId);
                parameters.Add("@ProductId", model.ProductId);
                parameters.Add("@SetupNodeId", model.SetupNodeId);
                parameters.Add("@ParentPath", model.ParentPath);
                parameters.Add("@DisplayName", model.DisplayName);
                parameters.Add("@ValueType", model.ValueType);
                parameters.Add("@ValueTypeId", model.ValueTypeId);
                parameters.Add("@ResultValue", model.ResultValue);
                parameters.Add("@IsActive", model.IsActive);
                parameters.Add("@CreatedBy", model.CreatedBy);
                parameters.Add("@ModifiedBy", model.ModifiedBy);

                using IDbConnection db = Connection;

                return await db.ExecuteScalarAsync<int>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save product entry.", ex);
            }
        }

        public async Task<int> DeleteAsync(Guid entryGroupId, string deletedBy)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "DELETE");
                parameters.Add("@EntryGroupId", entryGroupId);
                parameters.Add("@DeletedBy", deletedBy);

                using IDbConnection db = Connection;

                return await db.ExecuteScalarAsync<int>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete product entry.", ex);
            }
        }

        public async Task<int> ReportModel(int productId, string validSetupNodeIds, string deletedBy)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "CLEANREMOVEDITEMS");
                parameters.Add("@ProductId", productId);
                parameters.Add("@SetupNodeId", validSetupNodeIds);
                parameters.Add("@DeletedBy", deletedBy);

                using IDbConnection db = Connection;

                return await db.ExecuteScalarAsync<int>(
                    "sp_ProductEntry_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clean removed entry items.", ex);
            }
        }
    }
}