using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;

namespace EmployeeAccessSystem.Services
{
    public interface IProductSetupConfigurationService
    {
        Task<List<ProductSetupConfiguration>> GetTreeByProductIdAsync(int productId);
        Task<List<ProductConfiguration>> GetRootLevelsAsync(int productId);
        Task<List<ProductConfiguration>> GetChildLevelsAsync(int productId,int? parentConfigurationNodeId );
        Task<(bool Success, string Message)> SaveDataAsync(   ProductSetupConfigurationSaveRequest request,  string createdBy );
        Task<(bool Success, string Message)> DeleteByProductAsync(int productId,string deletedBy);
    }
}