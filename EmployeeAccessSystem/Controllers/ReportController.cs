using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeAccessSystem.Excel;
using EmployeeAccessSystem.Models;
using EmployeeAccessSystem.Pdf.Documents;
using EmployeeAccessSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace EmployeeAccessSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ReportController(
            IReportService reportService,
            IAccountService accountService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _reportService = reportService;
            _accountService = accountService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<ReportCategory> categoryList = await _reportService.GetCategoryListAsync();

            ViewBag.CategoryList = categoryList;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Headings = "";
            ViewBag.SenderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";

            var accounts = await _accountService.GetAllAccountsAsync();
            var userMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.Email))
                {
                    userMap[account.Email.Trim().ToLower()] = account.FullName ?? account.Email;
                }
            }
            ViewBag.UserMap = userMap;

            IEnumerable<Report> reportData = new List<Report>();

            if (categoryId.HasValue &&
                categoryId.Value > 0 &&
                fromDate.HasValue &&
                toDate.HasValue)
            {
                try
                {
                    string configurationJson = await _reportService.GetHeadingsAsync(categoryId.Value);
                    ViewBag.Headings = configurationJson;

                    reportData = await _reportService.GetDataAsync(
                        categoryId.Value,
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

        public async Task<IActionResult> AlertDashboard(int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<ReportCategory> categoryList = await _reportService.GetCategoryListAsync();

            ViewBag.CategoryList = categoryList;
            ViewBag.SelectedCategoryId = categoryId;
            
            // Default date range: last 30 days
            DateTime defaultFromDate = DateTime.Today.AddDays(-30);
            DateTime defaultToDate = DateTime.Today;

            DateTime resolvedFromDate = fromDate ?? defaultFromDate;
            DateTime resolvedToDate = toDate ?? defaultToDate;

            ViewBag.FromDate = resolvedFromDate;
            ViewBag.ToDate = resolvedToDate;

            var accounts = await _accountService.GetAllAccountsAsync();
            var userMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.Email))
                {
                    userMap[account.Email.Trim().ToLower()] = account.FullName ?? account.Email;
                }
            }
            ViewBag.UserMap = userMap;

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                try
                {
                    string configurationJson = await _reportService.GetHeadingsAsync(categoryId.Value);
                    ViewBag.Headings = configurationJson;
                }
                catch { }
            }

            List<AlertDisplayModel> alertsList = new List<AlertDisplayModel>();

            try
            {
                var reportData = await _reportService.GetAllDataAsync(resolvedFromDate, resolvedToDate, categoryId);
                
                foreach (var report in reportData)
                {
                    if (IsAlertEntry(report.ValueTypeId, report.ResultValue, out string formattedValue, out string reason))
                    {
                        string email = (report.CreatedBy ?? "").Trim().ToLower();
                        string creatorName = userMap.ContainsKey(email) ? userMap[email] : (report.CreatedBy ?? "-");

                        alertsList.Add(new AlertDisplayModel
                        {
                            EntryId = report.EntryId,
                            EntryGroupId = report.EntryGroupId,
                            CategoryId = report.CategoryId,
                            CategoryName = report.CategoryName ?? "General",
                            SetupNodeId = report.SetupNodeId,
                            ParentPath = report.ParentPath ?? "",
                            DisplayName = report.DisplayName ?? "",
                            ValueType = report.ValueType ?? "",
                            ValueTypeId = report.ValueTypeId,
                            RawValue = report.ResultValue ?? "",
                            DisplayValue = formattedValue,
                            EntryDate = report.EntryDate,
                            CreatedBy = report.CreatedBy ?? "",
                            CreatorName = creatorName,
                            TriggerReason = reason
                        });
                    }
                }

                // Calculate dashboard stats
                int totalChecks = 0;
                int uniqueMonitoredItems = 0;
                var categoryStats = new List<CategoryStatsModel>();

                if (reportData != null)
                {
                    var reportList = new List<Report>(reportData);
                    totalChecks = reportList.Count;
                    
                    var distinctNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var r in reportList)
                    {
                        if (!string.IsNullOrEmpty(r.SetupNodeId))
                        {
                            distinctNodes.Add(r.SetupNodeId);
                        }
                    }
                    uniqueMonitoredItems = distinctNodes.Count;

                    var grouped = new Dictionary<int, (string Name, int Total, int Alerts)>();
                    foreach (var r in reportList)
                    {
                        int catId = r.CategoryId;
                        string catName = r.CategoryName ?? "General";
                        
                        if (!grouped.ContainsKey(catId))
                        {
                            grouped[catId] = (catName, 0, 0);
                        }
                        
                        var entry = grouped[catId];
                        entry.Total++;
                        grouped[catId] = entry;
                    }

                    foreach (var a in alertsList)
                    {
                        int catId = a.CategoryId;
                        if (grouped.ContainsKey(catId))
                        {
                            var entry = grouped[catId];
                            entry.Alerts++;
                            grouped[catId] = entry;
                        }
                    }

                    foreach (var kvp in grouped)
                    {
                        int passed = kvp.Value.Total - kvp.Value.Alerts;
                        categoryStats.Add(new CategoryStatsModel
                        {
                            CategoryId = kvp.Key,
                            CategoryName = kvp.Value.Name,
                            TotalChecks = kvp.Value.Total,
                            PassedChecks = passed,
                            AlertsCount = kvp.Value.Alerts
                        });
                    }
                }

                int passedChecks = totalChecks - alertsList.Count;
                double overallPassRate = totalChecks > 0 ? (double)passedChecks / totalChecks * 100 : 100.0;
                double overallAlertRate = totalChecks > 0 ? (double)alertsList.Count / totalChecks * 100 : 0.0;

                ViewBag.TotalChecks = totalChecks;
                ViewBag.PassedChecks = passedChecks;
                ViewBag.OverallPassRate = overallPassRate;
                ViewBag.OverallAlertRate = overallAlertRate;
                ViewBag.MonitoredItemsCount = uniqueMonitoredItems;
                ViewBag.CategoryStats = categoryStats;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return View(alertsList);
        }

        public async Task<IActionResult> AlertDetails(string type, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<ReportCategory> categoryList = await _reportService.GetCategoryListAsync();

            ViewBag.CategoryList = categoryList;
            ViewBag.SelectedCategoryId = categoryId;
            
            DateTime defaultFromDate = DateTime.Today.AddDays(-30);
            DateTime defaultToDate = DateTime.Today;

            DateTime resolvedFromDate = fromDate ?? defaultFromDate;
            DateTime resolvedToDate = toDate ?? defaultToDate;

            ViewBag.FromDate = resolvedFromDate;
            ViewBag.ToDate = resolvedToDate;
            ViewBag.Type = type ?? "total";

            var accounts = await _accountService.GetAllAccountsAsync();
            var userMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.Email))
                {
                    userMap[account.Email.Trim().ToLower()] = account.FullName ?? account.Email;
                }
            }
            ViewBag.UserMap = userMap;

            List<AlertDisplayModel> alertsList = new List<AlertDisplayModel>();

            try
            {
                var reportData = await _reportService.GetAllDataAsync(resolvedFromDate, resolvedToDate, categoryId);
                
                foreach (var report in reportData)
                {
                    if (IsAlertEntry(report.ValueTypeId, report.ResultValue, out string formattedValue, out string reason))
                    {
                        string email = (report.CreatedBy ?? "").Trim().ToLower();
                        string creatorName = userMap.ContainsKey(email) ? userMap[email] : (report.CreatedBy ?? "-");

                        alertsList.Add(new AlertDisplayModel
                        {
                            EntryId = report.EntryId,
                            EntryGroupId = report.EntryGroupId,
                            CategoryId = report.CategoryId,
                            CategoryName = report.CategoryName ?? "General",
                            SetupNodeId = report.SetupNodeId,
                            ParentPath = report.ParentPath ?? "",
                            DisplayName = report.DisplayName ?? "",
                            ValueType = report.ValueType ?? "",
                            ValueTypeId = report.ValueTypeId,
                            RawValue = report.ResultValue ?? "",
                            DisplayValue = formattedValue,
                            EntryDate = report.EntryDate,
                            CreatedBy = report.CreatedBy ?? "",
                            CreatorName = creatorName,
                            TriggerReason = reason
                        });
                    }
                }

                // Filter list based on type
                if (string.Equals(type, "utilization", StringComparison.OrdinalIgnoreCase))
                {
                    alertsList = alertsList.Where(a => a.ValueTypeId == 3).ToList();
                }
                else if (string.Equals(type, "checklist", StringComparison.OrdinalIgnoreCase))
                {
                    alertsList = alertsList.Where(a => a.ValueTypeId == 4).ToList();
                }
                else if (string.Equals(type, "service", StringComparison.OrdinalIgnoreCase))
                {
                    alertsList = alertsList.Where(a => a.ValueTypeId == 5).ToList();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return View(alertsList);
        }

        private bool IsAlertEntry(int valueTypeId, string rawValue, out string formattedValue, out string reason)
        {
            formattedValue = "-";
            reason = "";
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
                    formattedValue = num.ToString("G29") + "%";
                    if (num >= 90.0)
                    {
                        reason = $"Value '{formattedValue}' is greater than or equal to the 90% threshold";
                        return true;
                    }
                }
            }
            else if (valueTypeId == 4) // Yes/No Checklist
            {
                if (val == "8" || val == "Tick" || val == "✓")
                {
                    formattedValue = "✓";
                    return false;
                }
                formattedValue = "✗";
                reason = "Checklist status marked as Fail (✗)";
                return true;
            }
            else if (valueTypeId == 5) // Up/Down Status
            {
                if (val == "10" || val == "Up")
                {
                    formattedValue = "Up";
                    return false;
                }
                formattedValue = "Down";
                reason = "Service status is Down";
                return true;
            }

            return false;
        }

        public async Task<IActionResult> DownloadExcel(int categoryId, DateTime fromDate, DateTime toDate)
        {
            DynamicReportExportModel model = await BuildExportModel(categoryId, fromDate, toDate);

            DynamicReportExcelDocument excel = new DynamicReportExcelDocument(model);

            byte[] fileBytes = excel.Generate();

            string fileName = "Report.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        public async Task<IActionResult> DownloadPdf(int categoryId, DateTime fromDate, DateTime toDate)
        {
            DynamicReportExportModel model = await BuildExportModel(categoryId, fromDate, toDate);

            DynamicReportPdfDocument document = new DynamicReportPdfDocument(model);

            byte[] fileBytes = document.GeneratePdf();

            string fileName = "Report.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        private async Task<DynamicReportExportModel> BuildExportModel(
            int categoryId,
            DateTime fromDate,
            DateTime toDate)
        {
            string headingJson = await _reportService.GetHeadingsAsync(categoryId);

            IEnumerable<Report> data = await _reportService.GetDataAsync(categoryId, fromDate, toDate);

            IEnumerable<ReportCategory> categoryList = await _reportService.GetCategoryListAsync();

            string categoryName = "";

            foreach (ReportCategory category in categoryList)
            {
                if (category.CategoryId == categoryId)
                {
                    categoryName = category.Name;
                    break;
                }
            }

            List<Report> reportData = new List<Report>();

            foreach (Report item in data)
            {
                reportData.Add(item);
            }

            var accounts = await _accountService.GetAllAccountsAsync();
            var userMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.Email))
                {
                    userMap[account.Email.Trim().ToLower()] = account.FullName ?? account.Email;
                }
            }

            DynamicReportExportModel model = new DynamicReportExportModel();

            model.Title = "Dynamic Category Report";
            model.CategoryName = categoryName;
            model.FromDate = fromDate.Date;
            model.ToDate = toDate.Date;
            model.Headings = BuildHeadings(headingJson);
            model.Dates = BuildDates(fromDate, toDate);
            model.Rows = BuildRows(reportData, model.Headings.Count);

            model.DateCreators = new Dictionary<string, string>();
            foreach (var date in model.Dates)
            {
                string dateKey = date.ToString("yyyy-MM-dd");
                
                var lastReport = reportData
                    .Where(r => r.EntryDate.Date == date.Date && !string.IsNullOrWhiteSpace(r.CreatedBy))
                    .OrderByDescending(r => r.EntryDate)
                    .ThenByDescending(r => r.EntryId)
                    .FirstOrDefault();

                string lastCreator = "";
                if (lastReport != null)
                {
                    string email = lastReport.CreatedBy.Trim().ToLower();
                    lastCreator = userMap.ContainsKey(email) ? userMap[email] : lastReport.CreatedBy;
                }
                
                model.DateCreators.Add(dateKey, lastCreator);
            }

            return model;
        }



        private List<DateTime> BuildDates(DateTime fromDate, DateTime toDate)
        {
            List<DateTime> dates = new List<DateTime>();

            DateTime currentDate = fromDate.Date;

            while (currentDate <= toDate.Date)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }

            return dates;
        }

        private List<string> BuildHeadings(string json)
        {
            List<string> headings = new List<string>();

            if (string.IsNullOrWhiteSpace(json))
            {
                return headings;
            }

            try
            {
                JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("structure", out JsonElement structure))
                {
                    AddLabels(structure, headings);
                }
                else if (root.TryGetProperty("Structure", out JsonElement capitalStructure))
                {
                    AddLabels(capitalStructure, headings);
                }
                else
                {
                    AddLabels(root, headings);
                }
            }
            catch
            {
                headings.Clear();
            }

            return headings;
        }

        private void AddLabels(JsonElement element, List<string> headings)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in element.EnumerateArray())
                {
                    AddLabels(child, headings);
                }

                return;
            }

            if (element.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            string label = "";

            if (element.TryGetProperty("label", out JsonElement labelElement))
            {
                label = labelElement.GetString();
            }
            else if (element.TryGetProperty("Label", out JsonElement capitalLabelElement))
            {
                label = capitalLabelElement.GetString();
            }

            if (!string.IsNullOrWhiteSpace(label))
            {
                headings.Add(label.Trim());
            }

            if (element.TryGetProperty("children", out JsonElement children))
            {
                AddLabels(children, headings);
            }
            else if (element.TryGetProperty("Children", out JsonElement capitalChildren))
            {
                AddLabels(capitalChildren, headings);
            }
        }

        private List<DynamicReportExportRow> BuildRows(List<Report> reportData, int headingCount)
        {
            List<DynamicReportExportRow> rows = new List<DynamicReportExportRow>();

            foreach (Report report in reportData)
            {
                List<string> leftValues = BuildLeftValues(report, headingCount);

                string rowKey = BuildRowKey(leftValues);

                DynamicReportExportRow row = null;

                foreach (DynamicReportExportRow existingRow in rows)
                {
                    if (BuildRowKey(existingRow.LeftValues) == rowKey)
                    {
                        row = existingRow;
                        break;
                    }
                }

                if (row == null)
                {
                    row = new DynamicReportExportRow();
                    row.LeftValues = leftValues;
                    rows.Add(row);
                }

                string dateKey = report.EntryDate.ToString("yyyy-MM-dd");

                string value = FormatResultValue(
                    report.ValueType,
                    report.ValueTypeId,
                    report.ResultValue
                );

                if (row.DateValues.ContainsKey(dateKey))
                {
                    row.DateValues[dateKey] = value;
                }
                else
                {
                    row.DateValues.Add(dateKey, value);
                }
            }

            return rows;
        }

        private List<string> BuildLeftValues(Report report, int headingCount)
        {
            List<string> values = new List<string>();

            if (!string.IsNullOrWhiteSpace(report.ParentPath))
            {
                string[] parts;

                if (report.ParentPath.Contains(">"))
                {
                    parts = report.ParentPath.Split('>');
                }
                else
                {
                    parts = report.ParentPath.Split('/');
                }

                foreach (string part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        values.Add(part.Trim());
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(report.DisplayName))
            {
                values.Add(report.DisplayName.Trim());
            }

            while (values.Count < headingCount)
            {
                values.Add("");
            }

            while (values.Count > headingCount)
            {
                values.RemoveAt(values.Count - 1);
            }

            return values;
        }

        private string BuildRowKey(List<string> values)
        {
            string key = "";

            foreach (string value in values)
            {
                key = key + value + "|";
            }

            return key;
        }

        private string FormatResultValue(string valueType, int valueTypeId, string resultValue)
        {
            if (string.IsNullOrWhiteSpace(resultValue))
            {
                return "-";
            }

            string value = resultValue.Trim();

            if (valueTypeId == 3)
            {
                if (value.EndsWith("%"))
                {
                    return value;
                }

                return value + "%";
            }

            if (valueTypeId == 4)
            {
                if (value == "8" || value == "Tick")
                {
                    return "✓";
                }

                return "✗";
            }

            if (valueTypeId == 5)
            {
                if (value == "10" || value == "Up")
                {
                    return "Up";
                }

                return "Down";
            }

            return value;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryDetailsForDate(int categoryId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    return Json(new { success = false, message = "Invalid date parameter." });
                }

                var data = await _reportService.GetDataAsync(categoryId, parsedDate, parsedDate);

                var accounts = await _accountService.GetAllAccountsAsync();
                var userMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var account in accounts)
                {
                    if (!string.IsNullOrWhiteSpace(account.Email))
                    {
                        userMap[account.Email.Trim().ToLower()] = account.FullName ?? account.Email;
                    }
                }

                var list = new List<object>();
                foreach (var item in data)
                {
                    string email = (item.CreatedBy ?? "").Trim().ToLower();
                    string creatorName = userMap.ContainsKey(email) ? userMap[email] : (item.CreatedBy ?? "-");

                    string displayValue = FormatResultValue(item.ValueType, item.ValueTypeId, item.ResultValue);

                    bool isAlert = false;
                    string reason = "";
                    string cleanVal = (item.ResultValue ?? "").Trim();
                    if (item.ValueTypeId == 3) // Percentage
                    {
                        string cleanPercentage = cleanVal.Replace("%", "").Trim();
                        if (double.TryParse(cleanPercentage, out double num) && num >= 90.0)
                        {
                            isAlert = true;
                            reason = $"Value '{displayValue}' is greater than or equal to the 90% threshold";
                        }
                    }
                    else if (item.ValueTypeId == 4) // Checklist
                    {
                        if (cleanVal != "8" && cleanVal != "Tick" && cleanVal != "✓")
                        {
                            isAlert = true;
                            reason = "Checklist status marked as Fail (✗)";
                        }
                    }
                    else if (item.ValueTypeId == 5) // Status
                    {
                        if (cleanVal != "10" && cleanVal != "Up")
                        {
                            isAlert = true;
                            reason = "Service status is Down";
                        }
                    }

                    list.Add(new
                    {
                        entryId = item.EntryId,
                        nodeId = item.SetupNodeId,
                        parentPath = item.ParentPath ?? "",
                        displayName = item.DisplayName ?? "",
                        valueType = item.ValueType ?? "",
                        valueTypeId = item.ValueTypeId,
                        rawValue = item.ResultValue ?? "",
                        displayValue = displayValue,
                        creatorName = creatorName,
                        creatorEmail = item.CreatedBy ?? "",
                        isAlert = isAlert,
                        reason = reason
                    });
                }

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to fetch details: " + ex.Message });
            }
        }
    }

    public class AlertDisplayModel
    {
        public int EntryId { get; set; }
        public Guid? EntryGroupId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string SetupNodeId { get; set; } = "";
        public string ParentPath { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string ValueType { get; set; } = "";
        public int ValueTypeId { get; set; }
        public string RawValue { get; set; } = "";
        public string DisplayValue { get; set; } = "";
        public DateTime EntryDate { get; set; }
        public string CreatedBy { get; set; } = "";
        public string CreatorName { get; set; } = "";
        public string TriggerReason { get; set; } = "";
    }

    public class CategoryStatsModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int AlertsCount { get; set; }
        public double PassRate => TotalChecks > 0 ? (double)PassedChecks / TotalChecks * 100 : 100.0;
        public double AlertRate => TotalChecks > 0 ? (double)AlertsCount / TotalChecks * 100 : 0.0;
    }
}