using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepositories _employeeRepo;

        public EmployeeService(IEmployeeRepositories employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _employeeRepo.GetAllAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _employeeRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetSupervisorsAsync()
        {
            return await _employeeRepo.GetSupervisorsAsync();
        }

        public async Task<string> UpdateAsync(Employee model)
        {
            if (model == null)
            {
                return "Invalid data";
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return "Full Name is required";
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return "Email is required";
            }

            if (model.DepartmentId <= 0)
            {
                return "Department is required";
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                return "Role is required";
            }

            model.FullName = model.FullName.Trim();
            model.Email = model.Email.Trim().ToLower();
            model.Role = model.Role.Trim();

            Employee? existing = await _employeeRepo.GetByEmailAsync(model.Email);

            if (existing != null && existing.EmployeeId != model.EmployeeId)
            {
                return "Email already used by another employee";
            }

            if (model.SupervisorEmployeeId == model.EmployeeId)
            {
                return "Employee cannot be their own supervisor";
            }

            int result = await _employeeRepo.UpdateAsync(model);

            if (result <= 0)
            {
                return "Employee update failed";
            }

            return "";
        }

        public async Task<string> ToggleAsync(int id)
        {
            Employee? emp = await _employeeRepo.GetByIdAsync(id);

            if (emp == null)
            {
                return "Employee not found";
            }

            await _employeeRepo.ToggleAsync(id);

            return "";
        }

        public async Task<string> DeleteAsync(int id)
        {
            Employee? emp = await _employeeRepo.GetByIdAsync(id);

            if (emp == null)
            {
                return "Employee not found";
            }

            int result = await _employeeRepo.DeleteAsync(id);

            if (result <= 0)
            {
                return "Employee delete failed";
            }

            return "";
        }
    }
}