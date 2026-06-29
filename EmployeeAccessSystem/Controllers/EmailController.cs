using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EmailController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private readonly IEmailService _emailService;

        public EmailController(
            IReportService reportService,
            IAccountService accountService,
            IEmailService emailService)
        {
            _reportService = reportService;
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReportEmail(
            int categoryId, 
            DateTime fromDate, 
            DateTime toDate, 
            string recipientEmail, 
            string fileType, 
            string subject, 
            string message, 
            string senderEmail = null, 
            string senderPassword = null)
        {
            try
            {
                recipientEmail = (recipientEmail ?? "").Trim();
                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    return Json(new { success = false, message = "Recipient email is required." });
                }

                if (string.IsNullOrWhiteSpace(fileType))
                {
                    return Json(new { success = false, message = "Please select report format." });
                }

                DynamicReportExportModel model = await BuildExportModel(categoryId, fromDate, toDate);

                byte[] fileBytes;
                string fileName;
                string contentType;

                if (string.Equals(fileType, "pdf", StringComparison.OrdinalIgnoreCase))
                {
                    DynamicReportPdfDocument document = new DynamicReportPdfDocument(model);
                    fileBytes = document.GeneratePdf();
                    fileName = "Report.pdf";
                    contentType = "application/pdf";
                }
                else if (string.Equals(fileType, "excel", StringComparison.OrdinalIgnoreCase))
                {
                    DynamicReportExcelDocument excel = new DynamicReportExcelDocument(model);
                    fileBytes = excel.Generate();
                    fileName = "Report.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    return Json(new { success = false, message = "Invalid report format selected." });
                }

                await _emailService.SendReportAttachmentEmailAsync(
                    recipientEmail,
                    "Recipient",
                    fileName,
                    fileBytes,
                    contentType,
                    subject,
                    message,
                    senderEmail,
                    senderPassword
                );

                return Json(new { success = true, message = "Report emailed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to email report: " + ex.Message });
            }
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
                foreach (JsonElement item in element.EnumerateArray())
                {
                    AddLabels(item, headings);
                }
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
    }
}
