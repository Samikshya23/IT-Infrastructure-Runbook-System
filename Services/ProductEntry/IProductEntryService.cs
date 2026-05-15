using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductEntryService
    {
        // Load all today's active product entries
        Task<IEnumerable<ProductEntryModel>> GetAllAsync();

        // Load today's entries by selected product
        Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId);

        // Load full details of one entry group
        Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId);

        // Load SetupJson from ProductSetupConfiguration
        Task<string> GetSetupAsync(int productId);

        // Load ConfigurationJson from ProductConfiguration
        Task<string> GetConfigurationAsync(int productId);

        // Load only products that already have setup configuration
        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        // Validate and save dynamic product entry values
        Task<string> SaveAsync(ProductEntrySaveRequest request, string createdBy);

        // Soft delete product entry group
        Task<string> DeleteAsync(Guid entryGroupId, string deletedBy);
    }
}