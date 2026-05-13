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
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<ReportProduct>> GetProductsAsync()
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETPRODUCTS");

                return await conn.QueryAsync<ReportProduct>(
                    "dbo.sp_Report_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                throw new Exception("Failed to load report products.");
            }
        }
        public async Task<string> GetHeadingsAsync(int productId)
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETHEADINGS");
                parameters.Add("ProductId", productId);

                return await conn.QueryFirstOrDefaultAsync<string>(
                    "dbo.sp_Report_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                throw new Exception("Failed to load report headings.");
            }
        }

        public async Task<IEnumerable<Report>> GetDataAsync(
            int productId,
            DateTime fromDate,
            DateTime toDate
        )
        {
            try
            {
                using var conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("Flag", "GETDATA");
                parameters.Add("ProductId", productId);
                parameters.Add("FromDate", fromDate.Date);
                parameters.Add("ToDate", toDate.Date);

                return await conn.QueryAsync<Report>(
                    "dbo.sp_Report_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                throw new Exception("Failed to load report data.");
            }
        }
    }
}