using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Repositories;

namespace EmployeeAccessSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        // Constructor
        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        // Load category list for report dropdown
        public async Task<IEnumerable<ReportCategory>> GetCategoryListAsync()
        {
            return await _reportRepository.GetCategoryListAsync();
        }

        // Load headings from Form Configuration
        public async Task<string> GetHeadingsAsync(int categoryId)
        {
            // Validate category
            if (categoryId <= 0)
            {
                throw new Exception("Please select a valid category.");
            }

            // Return headings from repository
            return await _reportRepository.GetHeadingsAsync(categoryId);
        }

        // Load report data from Category Checklist
        public async Task<IEnumerable<Report>> GetDataAsync(int categoryId, DateTime fromDate, DateTime toDate)
        {
            // Validate category
            if (categoryId <= 0)
            {
                throw new Exception("Please select a valid category.");
            }

            // Validate date range
            if (fromDate.Date > toDate.Date)
            {
                throw new Exception("From date cannot be greater than to date.");
            }

            int totalDays = (toDate.Date - fromDate.Date).Days;

            // Allow maximum 31 days only
            if (totalDays > 31)
            {
                throw new Exception("Maximum 31 days allowed.");
            }

            // Return report data from repository
            return await _reportRepository.GetDataAsync(categoryId, fromDate, toDate);
        }
    }
}