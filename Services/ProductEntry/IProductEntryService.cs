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

        Task<IEnumerable<ProductEntryModel>> GetByGroupAsync(Guid entryGroupId);

        Task<string> GetSetupJsonByProductAsync(int productId);

        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        Task<string> SaveEntryAsync(ProductEntrySaveRequest request, string createdBy);

        Task<string> DeleteGroupAsync(Guid entryGroupId, string deletedBy);
    }
}