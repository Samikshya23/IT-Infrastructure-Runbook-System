using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductSetupConfigurationService
    {
        Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId);

        Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId);
        Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId, int? parentConfigurationNodeId);

        Task<ProductSetupConfiguration> PrepareAddAsync(int productId, int? parentNodeId);
        Task<ProductSetupConfiguration> PrepareEditAsync(int nodeId);

        Task<(bool Success, string Message)> AddAsync(ProductSetupConfiguration model, string createdBy);
        Task<(bool Success, string Message)> UpdateNodeAsync(ProductSetupConfiguration model, string modifiedBy);
        Task<(bool Success, string Message)> SaveDataAsync(ProductSetupConfigurationSaveRequest request, string createdBy);
        Task<(bool Success, string Message)> DeleteNodeAsync(int nodeId, string deletedBy);
    }
}