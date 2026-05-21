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

        // Load category list for report dropdown
        public async Task<IEnumerable<ReportCategory>> GetCategoryListAsync()
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETCATEGORIES");

                return await conn.QueryAsync<ReportCategory>(
                    "dbo.sp_Report_Manage",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch
            {
                throw new Exception("Failed to load report category list.");
            }
        }

        // Load headings JSON from FormConfiguration
        public async Task<string> GetHeadingsAsync(int categoryId)
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETHEADINGS");
                parameters.Add("@CategoryId", categoryId);

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

        // Load report data from CategoryChecklist
        public async Task<IEnumerable<Report>> GetDataAsync(int categoryId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                using SqlConnection conn = GetConnection();

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Flag", "GETDATA");
                parameters.Add("@CategoryId", categoryId);
                parameters.Add("@FromDate", fromDate.Date);
                parameters.Add("@ToDate", toDate.Date);

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