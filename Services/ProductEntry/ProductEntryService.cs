using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductEntryService : IProductEntryService
    {
        private readonly IProductEntryRepository _productEntryRepository;

        public ProductEntryService(IProductEntryRepository productEntryRepository)
        {
            _productEntryRepository = productEntryRepository;
        }

        #region Load Methods

        // Load all today's active entries
        public async Task<IEnumerable<ProductEntryModel>> GetAllAsync()
        {
            return await _productEntryRepository.GetAllAsync();
        }

        // Load entries by selected record
        public async Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId)
        {
            return await _productEntryRepository.GetByProductAsync(productId);
        }

        // Load entry details by EntryGroupId
        public async Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId)
        {
            return await _productEntryRepository.GetDetailsAsync(entryGroupId);
        }

        // Load SetupJson from setup configuration
        public async Task<string> GetSetupAsync(int productId)
        {
            return await _productEntryRepository.GetSetupAsync(productId);
        }

        // Load ConfigurationJson from configuration setup
        public async Task<string> GetConfigurationAsync(int productId)
        {
            return await _productEntryRepository.GetConfigurationAsync(productId);
        }

        // Load records that already have setup configuration
        public async Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync()
        {
            return await _productEntryRepository.GetConfiguredProductsAsync();
        }

        #endregion

        #region Save Methods

        // Validate and save dynamic entry values
        public async Task<string> SaveAsync(ProductEntrySaveRequest request, string createdBy)
        {
            // Validate request object
            if (request == null)
            {
                return "Invalid request.";
            }

            // Validate selected record
            if (request.ProductId <= 0)
            {
                return "Please select record.";
            }

            // Validate item list
            if (request.Items == null || request.Items.Count == 0)
            {
                return "Please enter values.";
            }

            // Remove empty rows
            request.Items = request.Items
                .Where(x => !string.IsNullOrWhiteSpace(x.ResultValue))
                .ToList();

            // Validate rows after cleanup
            if (request.Items.Count == 0)
            {
                return "Please enter values.";
            }

            // Create single group id for same save operation
            Guid entryGroupId = Guid.NewGuid();

            foreach (ProductEntryFormItem item in request.Items)
            {
                // Skip invalid setup rows
                if (string.IsNullOrWhiteSpace(item.SetupNodeId))
                {
                    continue;
                }

                // Percentage validation
                if (item.ValueType == "Percentage")
                {
                    if (decimal.TryParse(item.ResultValue, out decimal percentage))
                    {
                        if (percentage > 100)
                        {
                            item.ResultValue = "100";
                        }

                        if (percentage < 0)
                        {
                            item.ResultValue = "0";
                        }
                    }
                }

                ProductEntryModel model = new ProductEntryModel
                {
                    EntryGroupId = entryGroupId,
                    ProductId = request.ProductId,
                    SetupNodeId = item.SetupNodeId,
                    ParentPath = item.ParentPath,
                    DisplayName = item.DisplayName,
                    ValueType = item.ValueType,
                    ValueTypeId = item.ValueTypeId,
                    ResultValue = item.ResultValue,
                    IsActive = true,
                    CreatedBy = createdBy,
                    ModifiedBy = createdBy
                };

                await _productEntryRepository.SaveAsync(model);
            }

            return "Entry saved successfully.";
        }

        #endregion

        #region Delete Methods

        // Soft delete entry group
        public async Task<string> DeleteAsync(Guid entryGroupId, string deletedBy)
        {
            int result = await _productEntryRepository.DeleteAsync(entryGroupId, deletedBy);

            if (result > 0)
            {
                return "Entry deleted successfully.";
            }

            return "Failed to delete entry.";
        }

        #endregion
    }
}