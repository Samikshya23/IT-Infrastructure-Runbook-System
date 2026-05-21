using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface ICategorySetupService
    {
        // Load configured categories
        Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync();

        // Load setup tree by category
        Task<List<CategorySetup>> GetTreeByCategoryIdAsync(int categoryId);

        // Load grouped setup tree
        Task<List<CategorySetup>> GetGroupedTreeByCategoryIdAsync(int categoryId);

        // Load root configuration levels
        Task<List<FormConfiguration>> GetRootLevelsAsync(int categoryId);

        // Load child configuration levels
        Task<List<FormConfiguration>> GetChildLevelsAsync(int categoryId, int? parentConfigurationNodeId);

        // Save setup data
        Task<(bool Success, string Message)> SaveDataAsync(CategorySetupSaveRequest request, string createdBy);

        // Delete setup by category
        Task<(bool Success, string Message)> DeleteByCategoryAsync(int categoryId, string deletedBy);

        // Load selected root group for edit
        Task<(bool Success, string Message, CategorySetupNodeRequest Data)> GetRootForEditAsync(int categoryId, int rootIndex);

        // Update selected root group
        Task<(bool Success, string Message)> SaveRootDataAsync(CategorySetupSaveRequest request, string modifiedBy);

        // Delete selected root group
        Task<(bool Success, string Message)> DeleteRootAsync(int categoryId, int rootIndex, string deletedBy);
    }
}