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

        Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId);

        Task<string> GetSetupAsync(int productId);

        Task<string> GetConfigurationAsync(int productId);

        Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync();

        Task<int> CheckExistsAsync(int productId);

        Task<int> SaveAsync(ProductEntryModel model);

        Task<int> DeleteAsync(Guid entryGroupId, string deletedBy);

        Task<int> ReportModel(int productId, string validSetupNodeIds, string deletedBy);
    }
}