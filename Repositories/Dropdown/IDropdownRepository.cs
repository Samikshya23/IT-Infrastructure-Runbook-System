using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Repositories
{
    public interface IDropdownRepository
    {
        Task<IEnumerable<DropdownGroupModel>> GetGroupsAsync();

        Task<IEnumerable<DropdownItemModel>> GetItemsAsync(int dropdownGroupId);

        Task<IEnumerable<DropdownItemModel>> GetItemsByGroupNameAsync(string groupName);
    }
}