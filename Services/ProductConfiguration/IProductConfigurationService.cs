using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductConfigurationService
    {
        Task<List<ProductConfigurationIndexItem>> GetIndexAsync();
        Task<List<ProductConfiguration>> GetTreeByProductIdAsync(int productId);
        Task<ProductConfiguration> GetNodeByIdAsync(int nodeId);

        Task<(bool Success, string Message)> SaveStructureAsync(ProductConfigurationSaveRequest request, string createdBy);
        Task<(bool Success, string Message)> UpdateNodeAsync(ProductConfiguration model);
        Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy);
        Task<(bool Success, string Message)> DeleteNodeAsync(int nodeId, string deletedBy);
    }
}