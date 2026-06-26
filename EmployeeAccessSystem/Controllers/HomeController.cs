using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAccessSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;

        public HomeController(
            ILogger<HomeController> logger,
            IReportService reportService,
            IAccountService accountService)
        {
            _logger = logger;
            _reportService = reportService;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Home page opened at " + DateTime.Now);

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var hasFullAccessClaim = User.FindFirst("HasFullAccess")?.Value;
                if (bool.TryParse(hasFullAccessClaim, out bool hasFullAccess) && hasFullAccess)
                {
                    return RedirectToAction("Index", "AccessControl");
                }

                // Compute stats for User Dashboard
                try
                {
                    // Last 30 days of data for stats
                    DateTime fromDate = DateTime.Today.AddDays(-30);
                    DateTime toDate = DateTime.Today;
                    var reportData = await _reportService.GetAllDataAsync(fromDate, toDate);

                    int activeAlertsCount = 0;

                    if (reportData != null)
                    {
                        var reportList = reportData.ToList();
                        foreach (var r in reportList)
                        {
                            if (IsAlertEntry(r.ValueTypeId, r.ResultValue))
                            {
                                activeAlertsCount++;
                            }
                        }
                    }

                    ViewBag.ActiveAlertsCount = activeAlertsCount;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard statistics.");
                    ViewBag.ActiveAlertsCount = 0;
                }

                return View("Dashboard");
            }

            return View();
        }

        private bool IsAlertEntry(int valueTypeId, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            string val = rawValue.Trim();

            if (valueTypeId == 3) // Percentage
            {
                string cleanVal = val.Replace("%", "").Trim();
                if (double.TryParse(cleanVal, out double num))
                {
                    if (num >= 90.0)
                    {
                        return true;
                    }
                }
            }
            else if (valueTypeId == 4) // Yes/No Checklist
            {
                if (val == "8" || val == "Tick" || val == "✓")
                {
                    return false;
                }
                return true; // Any state other than Tick/Yes is an alert
            }
            else if (valueTypeId == 5) // Up/Down Status
            {
                if (val == "10" || val == "Up")
                {
                    return false;
                }
                return true; // Down is an alert
            }

            return false;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
