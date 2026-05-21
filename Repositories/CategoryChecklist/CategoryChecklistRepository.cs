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
    public class CategoryChecklistRepository : ICategoryChecklistRepository
    {
        private readonly IConfiguration _configuration;

        public CategoryChecklistRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Create SQL connection
        private IDbConnection Connection => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        // Load all today's active checklist entries
        public async Task<IEnumerable<CategoryChecklistModel>> GetAllAsync()
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETALL");

                using IDbConnection db = Connection;
                return await db.QueryAsync<CategoryChecklistModel>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load checklist entries.", ex);
            }
        }

        // Load today's active entries by category
        public async Task<IEnumerable<CategoryChecklistModel>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETBYCATEGORY");
                parameters.Add("@CategoryId", categoryId);

                using IDbConnection db = Connection;
                return await db.QueryAsync<CategoryChecklistModel>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load checklist entries by category.", ex);
            }
        }

        // Load details of one entry group
        public async Task<IEnumerable<CategoryChecklistModel>> GetDetailsAsync(Guid entryGroupId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETDETAILS");
                parameters.Add("@EntryGroupId", entryGroupId);

                using IDbConnection db = Connection;
                return await db.QueryAsync<CategoryChecklistModel>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load checklist details.", ex);
            }
        }

        // Load SetupJson from CategorySetup
        public async Task<string> GetSetupAsync(int categoryId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETSETUP");
                parameters.Add("@CategoryId", categoryId);

                using IDbConnection db = Connection;
                return await db.QueryFirstOrDefaultAsync<string>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load category setup.", ex);
            }
        }

        // Load ConfigurationJson from FormConfiguration
        public async Task<string> GetConfigurationAsync(int categoryId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETCONFIGURATION");
                parameters.Add("@CategoryId", categoryId);

                using IDbConnection db = Connection;
                return await db.QueryFirstOrDefaultAsync<string>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load form configuration.", ex);
            }
        }

        // Load categories that already have saved setup
        public async Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync()
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETCONFIGUREDCATEGORIES");

                using IDbConnection db = Connection;
                return await db.QueryAsync<CategorySetup>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load configured categories.", ex);
            }
        }

        // Check whether today's entry already exists for category
        public async Task<int> CheckExistsAsync(int categoryId)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "CHECKEXISTS");
                parameters.Add("@CategoryId", categoryId);

                using IDbConnection db = Connection;
                return await db.ExecuteScalarAsync<int>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check checklist entry.", ex);
            }
        }

        // Save or update checklist entry value
        public async Task<int> SaveAsync(CategoryChecklistModel model)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "SAVE");
                parameters.Add("@EntryGroupId", model.EntryGroupId);
                parameters.Add("@CategoryId", model.CategoryId);
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
                return await db.ExecuteScalarAsync<int>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save checklist entry.", ex);
            }
        }

        // Soft delete checklist entry group
        public async Task<int> DeleteAsync(Guid entryGroupId, string deletedBy)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "DELETE");
                parameters.Add("@EntryGroupId", entryGroupId);
                parameters.Add("@DeletedBy", deletedBy);

                using IDbConnection db = Connection;
                return await db.ExecuteScalarAsync<int>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete checklist entry.", ex);
            }
        }

        // Report cleanup method; name kept same because report may use it later
        public async Task<int> ReportModel(int categoryId, string validSetupNodeIds, string deletedBy)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "CLEANREMOVEDITEMS");
                parameters.Add("@CategoryId", categoryId);
                parameters.Add("@SetupNodeId", validSetupNodeIds);
                parameters.Add("@DeletedBy", deletedBy);

                using IDbConnection db = Connection;
                return await db.ExecuteScalarAsync<int>("sp_CategoryChecklist_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clean removed checklist items.", ex);
            }
        }
    }
}