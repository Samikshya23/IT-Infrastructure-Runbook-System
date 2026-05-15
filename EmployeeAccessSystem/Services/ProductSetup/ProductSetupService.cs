using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Services
{
    public class ProductSetupService : IProductSetupService
    {
        private readonly IProductSetupRepositories _repo;
        public ProductSetupService(IProductSetupRepositories repo)
        {
            _repo = repo;
        }
        // Get all records
        public async Task<IEnumerable<ProductSetup>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
        // Get single record by id
        public async Task<ProductSetup> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
        // Save new record
        public async Task<string> AddAsync(ProductSetup productSetup)
        {
            if (productSetup == null)
            {
                return "Invalid request.";
            }

            if (string.IsNullOrWhiteSpace(productSetup.ProductName))
            {
                return "Name is required.";
            }

            productSetup.ProductName = productSetup.ProductName.Trim();

            if (productSetup.ProductName.Length > 100)
            {
                return "Name cannot exceed 100 characters.";
            }

            try
            {
                int result = await _repo.AddAsync(productSetup);

                if (result > 0)
                {
                    return "Record saved successfully.";
                }

                return "Failed to save record.";
            }
            catch (SqlException)
            {
                return "Database error while saving record.";
            }
            catch
            {
                return "Error while saving record.";
            }
        }
        // Update existing record
        public async Task<string> UpdateAsync(ProductSetup productSetup)
        {
            if (productSetup == null)
            {
                return "Invalid request.";
            }

            if (productSetup.ProductId <= 0)
            {
                return "Invalid record.";
            }

            if (string.IsNullOrWhiteSpace(productSetup.ProductName))
            {
                return "Name is required.";
            }

            productSetup.ProductName = productSetup.ProductName.Trim();

            if (productSetup.ProductName.Length > 100)
            {
                return "Name cannot exceed 100 characters.";
            }

            ProductSetup existing = await _repo.GetByIdAsync(productSetup.ProductId);

            if (existing == null)
            {
                return "Record not found.";
            }

            try
            {
                int result = await _repo.UpdateAsync(productSetup);

                if (result > 0)
                {
                    return "Record updated successfully.";
                }

                return "Failed to update record.";
            }
            catch (SqlException)
            {
                return "Database error while updating record.";
            }
            catch
            {
                return "Error while updating record.";
            }
        }

        // Delete record
        public async Task<string> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return "Invalid record.";
            }

            ProductSetup existing = await _repo.GetByIdAsync(id);

            if (existing == null)
            {
                return "Record not found.";
            }

            try
            {
                int result = await _repo.DeleteAsync(id);

                if (result > 0)
                {
                    return "Record deleted successfully.";
                }

                return "Failed to delete record.";
            }
            catch
            {
                return "This record cannot be deleted because it is used somewhere.";
            }
        }

        // Activate or deactivate record
        public async Task<string> ToggleAsync(int id)
        {
            if (id <= 0)
            {
                return "Invalid record.";
            }

            ProductSetup existing = await _repo.GetByIdAsync(id);

            if (existing == null)
            {
                return "Record not found.";
            }

            try
            {
                int result = await _repo.ToggleAsync(id);

                if (result > 0)
                {
                    return "Status updated successfully.";
                }

                return "Failed to update status.";
            }
            catch
            {
                return "Error while updating status.";
            }
        }
    }
}