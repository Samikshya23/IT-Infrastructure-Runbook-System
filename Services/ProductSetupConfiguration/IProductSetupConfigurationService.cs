using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductSetupConfigurationService
    {
        // Load configured products
        Task<IEnumerable<ProductSetupConfiguration>> GetConfiguredProductsAsync();

        // Load setup tree by product
        Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId);

        // Load grouped setup tree
        Task<List<ProductSetupConfiguration>> GetGroupedTreeByProductIdAsync(int productId);

        // Load root configuration levels
        Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId);

        // Load child configuration levels
        Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId, int? parentConfigurationNodeId);

        // Save setup data
        Task<(bool Success, string Message)> SaveDataAsync(ProductSetupConfigurationSaveRequest request, string createdBy);

        // Delete setup by product
        Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy);

        // Load selected root group for edit
        Task<(bool Success, string Message, ProductSetupConfigurationNodeRequest Data)> GetRootForEditAsync(int productId, int rootIndex);

        // Update selected root group
        Task<(bool Success, string Message)> SaveRootDataAsync(ProductSetupConfigurationSaveRequest request, string modifiedBy);

        // Delete selected root group
        Task<(bool Success, string Message)> DeleteRootAsync(int productId, int rootIndex, string deletedBy);
    }
}