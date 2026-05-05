using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductSetupConfigurationRepository
    {
        Task<IEnumerable<ProductSetupConfiguration>> GetByProductIdAsync(int productId);

        Task<ProductSetupConfiguration> GetJsonByProductIdAsync(int productId);

        Task<int> SaveOrUpdateJsonAsync(
            int productId,
            string setupJson,
            string createdBy
        );

        Task<int> DeleteJsonByProductAsync(int productId, string deletedBy);
    }
}