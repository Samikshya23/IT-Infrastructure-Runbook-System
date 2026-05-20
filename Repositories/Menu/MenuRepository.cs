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
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Load sidebar menus according to logged-in user's account permission
        public async Task<IEnumerable<MenuModel>> GetMenusByAccountIdAsync(int accountId)
        {
            try
            {
                using var conn = GetConnection();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETBYACCOUNT");
                parameters.Add("AccountId", accountId);

                return await conn.QueryAsync<MenuModel>("dbo.sp_Menu_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while loading sidebar menus.", ex);
            }
        }
    }
}