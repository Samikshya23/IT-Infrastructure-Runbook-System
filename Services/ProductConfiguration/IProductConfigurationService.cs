using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductConfigurationService
    {
        // Load configuration list for index page
        Task<List<ProductConfigurationIndexItem>> GetIndexAsync();

        // Load saved hierarchy structure by product
        Task<List<ProductConfiguration>> GetTreeByProductIdAsync(int productId);

        // Save or update configuration structure
        Task<(bool Success, string Message)> SaveStructureAsync(ProductConfigurationSaveRequest request, string createdBy);

        // Delete configuration by product
        Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy);
    }
}