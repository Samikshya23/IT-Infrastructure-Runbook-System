using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using EmployeeAccessSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EmployeeAccessSystem.Repositories
{
    public class DropdownRepository : IDropdownRepository
    {
        private readonly string _connectionString;

        public DropdownRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<DropdownGroupModel>> GetGroupsAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETGROUPS");

                return await conn.QueryAsync<DropdownGroupModel>(
                    "dbo.sp_Dropdown_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load dropdown groups.", ex);
            }
        }

        public async Task<IEnumerable<DropdownItemModel>> GetItemsAsync(int dropdownGroupId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETITEMS");
                parameters.Add("DropdownGroupId", dropdownGroupId);

                return await conn.QueryAsync<DropdownItemModel>(
                    "dbo.sp_Dropdown_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load dropdown items.", ex);
            }
        }

        public async Task<IEnumerable<DropdownItemModel>> GetItemsByGroupNameAsync(string groupName)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETITEMSBYGROUP");
                parameters.Add("GroupName", groupName);

                return await conn.QueryAsync<DropdownItemModel>(
                    "dbo.sp_Dropdown_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load dropdown items.", ex);
            }
        }
    }
}