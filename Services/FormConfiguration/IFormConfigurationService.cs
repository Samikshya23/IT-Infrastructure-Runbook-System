using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IFormConfigurationService
    {
        // Load configuration list for index page
        Task<List<FormConfigurationIndexItem>> GetIndexAsync();

        // Load saved hierarchy structure by category
        Task<List<FormConfiguration>> GetTreeByCategoryIdAsync(int categoryId);

        // Save or update configuration structure
        Task<(bool Success, string Message)> SaveStructureAsync(FormConfigurationSaveRequest request, string createdBy);

        // Delete configuration by category
        Task<(bool Success, string Message)> DeleteByCategoryAsync(int categoryId, string deletedBy);
    }
}