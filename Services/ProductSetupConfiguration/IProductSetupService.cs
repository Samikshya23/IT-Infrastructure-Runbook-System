using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductSetupConfigurationService
    {
        Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId);
        Task<ProductSetupConfiguration> GetNodeByIdAsync(int nodeId);
        Task<List<string>> GetNodeNameOptionsAsync(int productId);

        Task<(bool Success, string Message)> AddAsync(ProductSetupConfiguration model, string createdBy);
        Task<(bool Success, string Message)> UpdateNodeAsync(ProductSetupConfiguration model, string modifiedBy);
        Task<(bool Success, string Message)> DeleteNodeAsync(int nodeId, string deletedBy);
    }
}