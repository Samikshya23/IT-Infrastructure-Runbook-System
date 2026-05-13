using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductEntryService
    {
        Task<IEnumerable<ProductEntryModel>> GetAllAsync();

        Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId);

        Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId);

        Task<string> GetSetupAsync(int productId);

        Task<string> GetConfigurationAsync(int productId);

        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        Task<string> SaveAsync(ProductEntrySaveRequest request, string createdBy);

        Task<string> DeleteAsync(Guid entryGroupId, string deletedBy);
    }
}