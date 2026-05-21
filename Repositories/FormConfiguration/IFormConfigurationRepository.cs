using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IFormConfigurationRepository
    {
        Task<IEnumerable<FormConfiguration>> GetAllAsync();

        Task<FormConfiguration> GetJsonByCategoryIdAsync(int categoryId);

        Task<int> SaveOrUpdateJsonAsync(int categoryId, string configurationJson, string createdBy);

        Task<int> DeleteJsonByCategoryAsync(int categoryId, string deletedBy);
    }
}