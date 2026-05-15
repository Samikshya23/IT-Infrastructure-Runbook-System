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

        // Load products
        public async Task<IEnumerable<ReportProduct>> GetProductsAsync()
        {
            return await _reportRepository.GetProductsAsync();
        }

        // Load headings
        public async Task<string> GetHeadingsAsync(int productId)
        {
            // Validate product
            if (productId <= 0)
            {
                throw new Exception("Please select a valid product.");
            }

            // Return headings from repository
            return await _reportRepository.GetHeadingsAsync(productId);
        }

        // Load report data
        public async Task<IEnumerable<Report>> GetDataAsync(int productId, DateTime fromDate, DateTime toDate)
        {
            // Validate product
            if (productId <= 0)
            {
                throw new Exception("Please select a valid product.");
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
            return await _reportRepository.GetDataAsync(productId, fromDate, toDate);
        }
    }
}