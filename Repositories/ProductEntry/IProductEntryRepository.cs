using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IProductEntryRepository
    {
        Task<IEnumerable<ProductEntryModel>> GetAllAsync();

        Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId);

        Task<IEnumerable<ProductEntryModel>> GetByGroupAsync(Guid entryGroupId);

        Task<string> GetSetupJsonByProductAsync(int productId);

        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        Task<int> CheckExistsAsync(int productId, DateTime entryDate);

        Task AddAsync(ProductEntryModel model);

        Task DeleteGroupAsync(Guid entryGroupId, string deletedBy);
    }
}