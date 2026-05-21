using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeAccessSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepositories _repo;

        public CategoryService(ICategoryRepositories repo)
        {
            _repo = repo;
        }

        // Get all records
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        // Get single record by id
        public async Task<Category> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        // Save new record
        public async Task<string> AddAsync(Category category)
        {
            if (category == null)
            {
                return "Invalid request.";
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return "Name is required.";
            }

            category.Name = category.Name.Trim();

            if (category.Name.Length > 100)
            {
                return "Name cannot exceed 100 characters.";
            }

            try
            {
                int result = await _repo.AddAsync(category);

                if (result > 0)
                {
                    return "Added successfully.";
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
        public async Task<string> UpdateAsync(Category category)
        {
            if (category == null)
            {
                return "Invalid request.";
            }

            if (category.CategoryId <= 0)
            {
                return "Invalid record.";
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return "Name is required.";
            }

            category.Name = category.Name.Trim();

            if (category.Name.Length > 100)
            {
                return "Name cannot exceed 100 characters.";
            }

            Category existing = await _repo.GetByIdAsync(category.CategoryId);

            if (existing == null)
            {
                return "Record not found.";
            }

            try
            {
                int result = await _repo.UpdateAsync(category);

                if (result > 0)
                {
                    return "Updated successfully.";
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

            Category existing = await _repo.GetByIdAsync(id);

            if (existing == null)
            {
                return "Record not found.";
            }

            try
            {
                int result = await _repo.DeleteAsync(id);

                if (result > 0)
                {
                    return "Deleted successfully.";
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

            Category existing = await _repo.GetByIdAsync(id);

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