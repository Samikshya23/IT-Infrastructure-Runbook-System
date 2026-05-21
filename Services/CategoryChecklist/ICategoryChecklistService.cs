using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface ICategoryChecklistService
    {
        // Load all today's active checklist entries
        Task<IEnumerable<CategoryChecklistModel>> GetAllAsync();

        // Load today's entries by selected category
        Task<IEnumerable<CategoryChecklistModel>> GetByCategoryAsync(int categoryId);

        // Load full details of one entry group
        Task<IEnumerable<CategoryChecklistModel>> GetDetailsAsync(Guid entryGroupId);

        // Load SetupJson from CategorySetup
        Task<string> GetSetupAsync(int categoryId);

        // Load ConfigurationJson from FormConfiguration
        Task<string> GetConfigurationAsync(int categoryId);

        // Load only categories that already have setup configuration
        Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync();

        // Validate and save dynamic checklist values
        Task<string> SaveAsync(CategoryChecklistSaveRequest request, string createdBy);

        // Soft delete checklist entry group
        Task<string> DeleteAsync(Guid entryGroupId, string deletedBy);
    }
}