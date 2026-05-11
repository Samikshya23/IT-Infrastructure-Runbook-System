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
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection()
        {
            SqlConnection connection =
                new SqlConnection(_connectionString);

            return connection;
        }

        public async Task<IEnumerable<MenuModel>> GetMenusByAccountIdAsync(int accountId)
        {
            using SqlConnection conn = GetConnection();

            DynamicParameters parameters =
                new DynamicParameters();

            parameters.Add("Flag", "GETBYACCOUNT");
            parameters.Add("AccountId", accountId);

            IEnumerable<MenuModel> result =
                await conn.QueryAsync<MenuModel>(
                    "dbo.sp_Menu_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

            return result;
        }
    }
}