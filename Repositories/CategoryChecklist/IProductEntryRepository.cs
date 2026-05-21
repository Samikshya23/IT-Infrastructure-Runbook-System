using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductEntryRepository
    {
        // Load all today's product entries
        Task<IEnumerable<ProductEntryModel>> GetAllAsync();

        // Load today's entries by product
        Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId);

        // Load entry details by group
        Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId);

        // Load setup JSON for selected product
        Task<string> GetSetupAsync(int productId);

        // Load configuration JSON for selected product
        Task<string> GetConfigurationAsync(int productId);

        // Load products that have setup configuration
        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        // Check today's entry exists or not
        Task<int> CheckExistsAsync(int productId);

        // Save product entry value
        Task<int> SaveAsync(ProductEntryModel model);

        // Soft delete entry group
        Task<int> DeleteAsync(Guid entryGroupId, string deletedBy);

        // Used by report / cleanup logic
        Task<int> ReportModel(int productId, string validSetupNodeIds, string deletedBy);
    }
}