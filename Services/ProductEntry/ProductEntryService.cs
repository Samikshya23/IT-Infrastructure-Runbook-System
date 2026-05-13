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
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<ProductEntryModel>> GetByProductAsync(int productId)
        {
            return await _repository.GetByProductAsync(productId);
        }

        public async Task<IEnumerable<ProductEntryModel>> GetDetailsAsync(Guid entryGroupId)
        {
            return await _repository.GetDetailsAsync(entryGroupId);
        }

        public async Task<string> GetSetupAsync(int productId)
        {
            if (productId <= 0)
            {
                return "";
            }

            return await _repository.GetSetupAsync(productId);
        }

        public async Task<string> GetConfigurationAsync(int productId)
        {
            if (productId <= 0)
            {
                return "";
            }

            return await _repository.GetConfigurationAsync(productId);
        }

        public async Task<IEnumerable<ProductConfigurationIndexItem>> GetConfiguredProductsAsync()
        {
            return await _repository.GetConfiguredProductsAsync();
        }

        public async Task<string> SaveAsync(ProductEntrySaveRequest request, string createdBy)
        {
            if (request == null)
            {
                return "Invalid request.";
            }

            if (request.ProductId <= 0)
            {
                return "Please select a main item.";
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return "No entry items found.";
            }

            foreach (ProductEntryFormItem item in request.Items)
            {
                if (item == null)
                {
                    return "Invalid entry item found.";
                }

                if (string.IsNullOrWhiteSpace(item.ResultValue))
                {
                    return "Please fill all required values.";
                }

                if (!ValidatePercentage(item, out string validationMessage))
                {
                    return validationMessage;
                }
            }

            int existingCount = await _repository.CheckExistsAsync(request.ProductId);

            Guid entryGroupId = request.EntryGroupId ?? Guid.NewGuid();

            int savedCount = 0;

            foreach (ProductEntryFormItem item in request.Items)
            {
                ProductEntryModel model = new ProductEntryModel();

                model.EntryGroupId = entryGroupId;
                model.ProductId = request.ProductId;
                model.SetupNodeId = item.SetupNodeId;
                model.ParentPath = item.ParentPath;
                model.DisplayName = item.DisplayName;
                model.ValueType = item.ValueType;
                model.ValueTypeId = item.ValueTypeId;
                model.ResultValue = item.ResultValue;
                model.IsActive = true;
                model.CreatedBy = createdBy;
                model.ModifiedBy = createdBy;

                int result = await _repository.SaveAsync(model);

                if (result > 0)
                {
                    savedCount++;
                }
            }

            if (savedCount == 0)
            {
                return "Failed to save.";
            }

            if (existingCount > 0)
            {
                return "Entry updated successfully.";
            }

            return "Entry added successfully.";
        }

        public async Task<string> DeleteAsync(Guid entryGroupId, string deletedBy)
        {
            if (entryGroupId == Guid.Empty)
            {
                return "Invalid entry.";
            }

            int result = await _repository.DeleteAsync(entryGroupId, deletedBy);

            if (result > 0)
            {
                return "Entry deleted successfully.";
            }

            return "Failed to delete.";
        }

        private bool ValidatePercentage(ProductEntryFormItem item, out string message)
        {
            message = "";

            bool isPercentageById = item.ValueTypeId == 3;

            bool isPercentageByText =
                item.ValueType != null &&
                item.ValueType.Trim().ToLower() == "percentage";

            if (!isPercentageById && !isPercentageByText)
            {
                return true;
            }

            string resultValue = item.ResultValue.Replace("%", "").Trim();

            decimal percentageValue;

            if (!decimal.TryParse(resultValue, out percentageValue))
            {
                message = "Invalid percentage value.";
                return false;
            }

            if (percentageValue < 0 || percentageValue > 100)
            {
                message = "Percentage value must be between 0 and 100.";
                return false;
            }

            item.ResultValue = percentageValue.ToString();

            return true;
        }
    }
}