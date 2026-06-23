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

        public async Task<IEnumerable<Report>> GetAllDataAsync(DateTime fromDate, DateTime toDate, int? categoryId = null)
        {
            try
            {
                using SqlConnection conn = GetConnection();
                var sql = @"
                    SELECT 
                        CC.EntryId,
                        CC.EntryGroupId,
                        CC.CategoryId,
                        C.Name AS CategoryName,
                        CC.SetupNodeId,
                        CC.ParentPath,
                        CC.DisplayName,
                        CC.ValueType,
                        CC.ValueTypeId,
                        CC.ResultValue,
                        CC.EntryDate AS EntryDate,
                        CC.CreatedBy
                    FROM dbo.CategoryChecklist CC WITH (NOLOCK)
                    INNER JOIN dbo.Category C WITH (NOLOCK) ON CC.CategoryId = C.CategoryId
                    WHERE CC.IsActive = 1
                      AND CAST(CC.EntryDate AS DATE) BETWEEN @FromDate AND @ToDate";

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    sql += " AND CC.CategoryId = @CategoryId";
                }

                sql += " ORDER BY CC.EntryDate DESC, CC.ParentPath, CC.DisplayName;";

                var parameters = new DynamicParameters();
                parameters.Add("@FromDate", fromDate.Date);
                parameters.Add("@ToDate", toDate.Date);
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    parameters.Add("@CategoryId", categoryId.Value);
                }

                return await conn.QueryAsync<Report>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load all report data for alert dashboard.", ex);
            }
        }
    }
}