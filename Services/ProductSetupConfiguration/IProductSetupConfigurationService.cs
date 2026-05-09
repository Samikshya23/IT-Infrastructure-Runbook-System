using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductSetupConfigurationService
    {
        Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId);

        Task<List<ProductSetupConfiguration>> GetGroupedTreeByProductIdAsync(int productId);

        Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId);

        Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId, int? parentConfigurationNodeId);

        Task<(bool Success, string Message)> SaveDataAsync(ProductSetupConfigurationSaveRequest request, string createdBy);

        Task<(bool Success, string Message)> DeleteByProductAsync(int productId, string deletedBy);

        //  Load only selected root group like Log or Telnet
        Task<(bool Success, string Message, ProductSetupConfigurationNodeRequest Data)>
            GetRootForEditAsync(int productId, int rootIndex);

        //  Update only selected root group
        Task<(bool Success, string Message)> SaveRootDataAsync(ProductSetupConfigurationSaveRequest request, string modifiedBy);

        //  Delete only selected root group
        Task<(bool Success, string Message)> DeleteRootAsync(int productId, int rootIndex,
     string deletedBy);
    }
}