using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ProductEntryService : IProductEntryService
    {
        private readonly IProductEntryRepository _repository;

        public ProductEntryService(IProductEntryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductEntryModel>> GetAllAsync()
        {
            IEnumerable<ProductEntryModel> data =
                await _repository.GetAllAsync();

            return data;
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId)
        {
            IEnumerable<ProductEntryModel> data =
                await _repository.GetByProductAsync(productId);

            return data;
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByGroupAsync(Guid entryGroupId)
        {
            IEnumerable<ProductEntryModel> data =
                await _repository.GetByGroupAsync(entryGroupId);

            return data;
        }

        public async Task<string> GetSetupJsonByProductAsync(int productId)
        {
            if (productId <= 0)
            {
                return "";
            }

            string setupJson =
                await _repository.GetSetupJsonByProductAsync(productId);

            if (string.IsNullOrWhiteSpace(setupJson))
            {
                return "";
            }

            return setupJson;
        }

        public async Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync()
        {
            IEnumerable<ProductConfigurationIndexItem> products =
                await _repository.GetConfiguredProductsAsync();

            return products;
        }

        public async Task<string> SaveEntryAsync(ProductEntrySaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return "Invalid product entry data.";
            }

            if (request.ProductId <= 0)
            {
                return "Please select product.";
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return "No product entry item found.";
            }

            if (string.IsNullOrWhiteSpace(createdBy))
            {
                createdBy = "System";
            }

            DateTime entryDate = DateTime.Now;

            if (!request.EntryGroupId.HasValue)
            {
                int existingCount =
                    await _repository.CheckExistsAsync(
                        request.ProductId,
                        entryDate);

                if (existingCount > 0)
                {
                    return "Entry already exists for today.";
                }
            }

            Guid entryGroupId;

            if (request.EntryGroupId.HasValue)
            {
                entryGroupId = request.EntryGroupId.Value;

                await _repository.DeleteGroupAsync(entryGroupId, createdBy);
            }
            else
            {
                entryGroupId = Guid.NewGuid();
            }

            foreach (ProductEntryFormItem item in request.Items)
            {
                if (item == null)
                {
                    continue;
                }

                string resultValue = item.ResultValue;

                if (item.ValueType == "Percentage")
                {
                    decimal percentageValue;

                    if (decimal.TryParse(resultValue, out percentageValue))
                    {
                        if (percentageValue > 100)
                        {
                            return "Percentage value cannot be greater than 100.";
                        }

                        if (percentageValue < 0)
                        {
                            return "Percentage value cannot be less than 0.";
                        }

                        resultValue = percentageValue.ToString();
                    }
                    else if (!string.IsNullOrWhiteSpace(resultValue))
                    {
                        return "Invalid percentage value.";
                    }
                }

                ProductEntryModel entry = new ProductEntryModel();

                entry.EntryGroupId = entryGroupId;
                entry.ProductId = request.ProductId;
                entry.SetupNodeId = item.SetupNodeId;
                entry.ParentPath = item.ParentPath;
                entry.DisplayName = item.DisplayName;
                entry.ValueType = item.ValueType;
                entry.ResultValue = resultValue;
                entry.EntryDate = entryDate;
                entry.IsActive = true;
                entry.CreatedBy = createdBy;

                await _repository.AddAsync(entry);
            }

            return "Product entry saved successfully.";
        }

        public async Task<string> DeleteGroupAsync(Guid entryGroupId, string deletedBy)
        {
            if (entryGroupId == Guid.Empty)
            {
                return "Invalid product entry.";
            }

            if (string.IsNullOrWhiteSpace(deletedBy))
            {
                deletedBy = "System";
            }

            await _repository.DeleteGroupAsync(entryGroupId, deletedBy);

            return "Product entry deleted successfully.";
        }
    }
}