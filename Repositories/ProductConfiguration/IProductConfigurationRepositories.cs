using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductConfigurationRepository
    {
        Task<IEnumerable<ProductConfiguration>> GetAllAsync();

        Task<ProductConfiguration> GetJsonByProductIdAsync(int productId);

        Task<int> SaveOrUpdateJsonAsync(int productId, string configurationJson, string createdBy);

        Task<int> DeleteJsonByProductAsync(int productId, string deletedBy);
    }
}