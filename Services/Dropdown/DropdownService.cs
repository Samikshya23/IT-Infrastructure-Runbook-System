using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class DropdownService : IDropdownService
    {
        private readonly IDropdownRepository _repository;

        public DropdownService(IDropdownRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DropdownGroupModel>> GetGroupsAsync()
        {
            return await _repository.GetGroupsAsync();
        }

        public async Task<IEnumerable<DropdownItemModel>> GetItemsAsync(int dropdownGroupId)
        {
            if (dropdownGroupId <= 0)
            {
                return new List<DropdownItemModel>();
            }

            return await _repository.GetItemsAsync(dropdownGroupId);
        }

        public async Task<IEnumerable<DropdownItemModel>> GetItemsByGroupNameAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return new List<DropdownItemModel>();
            }

            return await _repository.GetItemsByGroupNameAsync(groupName);
        }
    }
}