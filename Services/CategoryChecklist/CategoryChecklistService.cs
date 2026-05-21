using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class CategoryChecklistService : ICategoryChecklistService
    {
        private readonly ICategoryChecklistRepository _repository;

        public CategoryChecklistService(ICategoryChecklistRepository repository)
        {
            _repository = repository;
        }

        #region Load Methods

        // Load all today's active entries
        public async Task<IEnumerable<CategoryChecklistModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // Load entries by selected category
        public async Task<IEnumerable<CategoryChecklistModel>> GetByCategoryAsync(int categoryId)
        {
            return await _repository.GetByCategoryAsync(categoryId);
        }

        // Load entry details by EntryGroupId
        public async Task<IEnumerable<CategoryChecklistModel>> GetDetailsAsync(Guid entryGroupId)
        {
            return await _repository.GetDetailsAsync(entryGroupId);
        }

        // Load SetupJson from category setup
        public async Task<string> GetSetupAsync(int categoryId)
        {
            return await _repository.GetSetupAsync(categoryId);
        }

        // Load ConfigurationJson from form configuration
        public async Task<string> GetConfigurationAsync(int categoryId)
        {
            return await _repository.GetConfigurationAsync(categoryId);
        }

        // Load categories that already have setup configuration
        public async Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync()
        {
            return await _repository.GetConfiguredCategoriesAsync();
        }

        #endregion

        #region Save Methods

        // Validate and save dynamic checklist values
        public async Task<string> SaveAsync(CategoryChecklistSaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return "Invalid request.";
            }

            if (request.CategoryId <= 0)
            {
                return "Please select record.";
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return "Please enter values.";
            }

            request.Items = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.ResultValue))
                .ToList();

            if (request.Items.Count == 0)
            {
                return "Please enter values.";
            }

            Guid entryGroupId = Guid.NewGuid();

            if (request.EntryGroupId != null && request.EntryGroupId != Guid.Empty)
            {
                entryGroupId = request.EntryGroupId.Value;
            }

            foreach (CategoryChecklistFormItem item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.SetupNodeId))
                {
                    continue;
                }

                if (item.ValueType == "Percentage")
                {
                    if (decimal.TryParse(item.ResultValue, out decimal percentage))
                    {
                        if (percentage > 100)
                        {
                            item.ResultValue = "100";
                        }

                        if (percentage < 0)
                        {
                            item.ResultValue = "0";
                        }
                    }
                }

                CategoryChecklistModel model = new CategoryChecklistModel
                {
                    EntryGroupId = entryGroupId,
                    CategoryId = request.CategoryId,
                    SetupNodeId = item.SetupNodeId,
                    ParentPath = item.ParentPath,
                    DisplayName = item.DisplayName,
                    ValueType = item.ValueType,
                    ValueTypeId = item.ValueTypeId,
                    ResultValue = item.ResultValue,
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                await _repository.SaveAsync(model);
            }

            return "Saved successfully.";
        }

        #endregion

        #region Delete Methods

        // Soft delete entry group
        public async Task<string> DeleteAsync(Guid entryGroupId, string deletedBy)
        {
            int result = await _repository.DeleteAsync(entryGroupId, deletedBy);

            if (result > 0)
            {
                return "Deleted successfully.";
            }

            return "Delete failed.";
        }

        #endregion
    }
}