using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface ICategoryChecklistRepository
    {
        // Load all today's checklist entries
        Task<IEnumerable<CategoryChecklistModel>> GetAllAsync();

        // Load today's entries by category
        Task<IEnumerable<CategoryChecklistModel>> GetByCategoryAsync(int categoryId);

        // Load entry details by group
        Task<IEnumerable<CategoryChecklistModel>> GetDetailsAsync(Guid entryGroupId);

        // Load setup JSON for selected category
        Task<string> GetSetupAsync(int categoryId);

        // Load configuration JSON for selected category
        Task<string> GetConfigurationAsync(int categoryId);

        // Load categories that have setup configuration
        Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync();

        // Check today's entry exists or not
        Task<int> CheckExistsAsync(int categoryId);

        // Save checklist value
        Task<int> SaveAsync(CategoryChecklistModel model);

        // Soft delete entry group
        Task<int> DeleteAsync(Guid entryGroupId, string deletedBy);

        // Used by report / cleanup logic
        Task<int> ReportModel(int categoryId, string validSetupNodeIds, string deletedBy);
    }
}