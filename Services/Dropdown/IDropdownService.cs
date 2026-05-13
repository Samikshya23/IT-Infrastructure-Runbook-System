using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IDropdownService
    {
        Task<IEnumerable<DropdownGroupModel>> GetGroupsAsync();

        Task<IEnumerable<DropdownItemModel>> GetItemsAsync(int dropdownGroupId);

        Task<IEnumerable<DropdownItemModel>> GetItemsByGroupNameAsync(string groupName);
    }
}