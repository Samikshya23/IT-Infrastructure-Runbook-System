using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(
            int? productId,
            DateTime? fromDate,
            DateTime? toDate
        )
        {
            ViewBag.Products = await _reportService.GetProductsAsync();
            ViewBag.SelectedProductId = productId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Headings = "";

            IEnumerable<Report> reportData = new List<Report>();

            if (productId.HasValue &&
                fromDate.HasValue &&
                toDate.HasValue)
            {
                try
                {
                    ViewBag.Headings =
                        await _reportService.GetHeadingsAsync(productId.Value);

                    reportData =
                        await _reportService.GetDataAsync(
                            productId.Value,
                            fromDate.Value,
                            toDate.Value
                        );
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(reportData);
        }
    }
}