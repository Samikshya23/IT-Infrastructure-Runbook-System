using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface ICategorySetupRepository
    {
        Task<IEnumerable<CategorySetup>> GetAllAsync();

        Task<IEnumerable<CategorySetup>> GetConfiguredCategoriesAsync();

        Task<IEnumerable<CategorySetup>> GetByCategoryIdAsync(int categoryId);

        Task<CategorySetup> GetJsonByCategoryIdAsync(int categoryId);

        Task<int> SaveOrUpdateJsonAsync(int categoryId, string setupJson, string createdBy, string modifiedBy);

        Task<int> DeleteJsonByCategoryAsync(int categoryId, string deletedBy);
    }
}