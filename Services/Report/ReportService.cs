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

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<IEnumerable<ReportProduct>> GetProductsAsync()
        {
            return await _reportRepository.GetProductsAsync();
        }

        public async Task<string> GetHeadingsAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new Exception("Please select a valid product.");
            }

            return await _reportRepository.GetHeadingsAsync(productId);
        }

        public async Task<IEnumerable<Report>> GetDataAsync(
            int productId,
            DateTime fromDate,
            DateTime toDate
        )
        {
            if (productId <= 0)
            {
                throw new Exception("Please select a valid product.");
            }

            if (fromDate.Date > toDate.Date)
            {
                throw new Exception("From date cannot be greater than to date.");
            }

            int totalDays = (toDate.Date - fromDate.Date).Days;

            if (totalDays > 31)
            {
                throw new Exception("Maximum 31 days allowed.");
            }

            return await _reportRepository.GetDataAsync(
                productId,
                fromDate,
                toDate
            );
        }
    }
}