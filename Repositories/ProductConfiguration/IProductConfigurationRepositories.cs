using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductConfigurationRepository
    {
        Task<IEnumerable<ProductConfiguration>> GetAllAsync();
        Task<IEnumerable<ProductConfiguration>> GetByProductIdAsync(int productId);
        Task<ProductConfiguration> GetNodeByIdAsync(int nodeId);
        Task<int> AddAsync(ProductConfiguration model);
        Task<int> UpdateNodeAsync(ProductConfiguration model);
        Task<int> DeleteByProductAsync(int productId, string deletedBy);
        Task<int> DeleteNodeAsync(int nodeId, string deletedBy);
    }
}