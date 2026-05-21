using EmployeeAccessSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductSetupRepositories
    {
        Task<IEnumerable<ProductSetup>> GetAllAsync();
        Task<IEnumerable<ProductSetup>> GetActiveAsync();
        Task<ProductSetup> GetByIdAsync(int id);
        Task<int> AddAsync(ProductSetup productSetup);
        Task<int> UpdateAsync(ProductSetup productSetup);
        Task<int> DeleteAsync(int id);
        Task<int> ToggleAsync(int id);
    }
}