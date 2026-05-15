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

        // Constructor
        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Create SQL connection
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Load report product list
        public async Task<IEnumerable<ReportProduct>> GetProductsAsync()
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETPRODUCTS");

                // Execute stored procedure and return product list
                return await conn.QueryAsync<ReportProduct>("dbo.sp_Report_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch
            {
                throw new Exception("Failed to load report products.");
            }
        }

        // Load headings JSON
        public async Task<string> GetHeadingsAsync(int productId)
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETHEADINGS");
                parameters.Add("@ProductId", productId);

                // Execute stored procedure and return headings
                return await conn.QueryFirstOrDefaultAsync<string>("dbo.sp_Report_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch
            {
                throw new Exception("Failed to load report headings.");
            }
        }

        // Load report data
        public async Task<IEnumerable<Report>> GetDataAsync(int productId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETDATA");
                parameters.Add("@ProductId", productId);
                parameters.Add("@FromDate", fromDate.Date);
                parameters.Add("@ToDate", toDate.Date);

                // Execute stored procedure and return report data
                return await conn.QueryAsync<Report>("dbo.sp_Report_Manage", parameters, commandType: CommandType.StoredProcedure);
            }
            catch
            {
                throw new Exception("Failed to load report data.");
            }
        }
    }
}