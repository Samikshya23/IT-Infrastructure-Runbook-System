using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductSetupConfigurationRepository
    {
        Task<IEnumerable<ProductSetupConfiguration>> GetByProductIdAsync(int productId);
        Task<ProductSetupConfiguration> GetNodeByIdAsync(int nodeId);
        Task<int> CheckDuplicateNodeAsync(int productId, int configurationNodeId, int? parentNodeId, string nodeValue);
        Task<int> AddAsync(ProductSetupConfiguration model);
        Task<int> UpdateNodeAsync(ProductSetupConfiguration model);
        Task<int> DeleteNodeAsync(int nodeId, string deletedBy);
    }
}