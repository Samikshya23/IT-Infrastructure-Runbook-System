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

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Report page
        public async Task<IActionResult> Index(int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<ReportCategory> categoryList = await _reportService.GetCategoryListAsync();

            ViewBag.CategoryList = categoryList;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Headings = "";

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

        // Download Excel report
        public async Task<IActionResult> DownloadExcel(int categoryId, DateTime fromDate, DateTime toDate)
        {
            DynamicReportExportModel model = await BuildExportModel(categoryId, fromDate, toDate);

            DynamicReportExcelDocument excel = new DynamicReportExcelDocument(model);

            byte[] fileBytes = excel.Generate();

            string fileName =
                "Category_Report_" +
                fromDate.ToString("yyyyMMdd") +
                "_" +
                toDate.ToString("yyyyMMdd") +
                ".xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // Download PDF report
        public async Task<IActionResult> DownloadPdf(int categoryId, DateTime fromDate, DateTime toDate)
        {
            DynamicReportExportModel model = await BuildExportModel(categoryId, fromDate, toDate);

            DynamicReportPdfDocument document = new DynamicReportPdfDocument(model);

            byte[] fileBytes = document.GeneratePdf();

            string fileName =
                "Category_Report_" +
                fromDate.ToString("yyyyMMdd") +
                "_" +
                toDate.ToString("yyyyMMdd") +
                ".pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        // Build export model for PDF and Excel
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

            DynamicReportExportModel model = new DynamicReportExportModel();

            model.Title = "Dynamic Category Report";
            model.CategoryName = categoryName;
            model.FromDate = fromDate.Date;
            model.ToDate = toDate.Date;
            model.Headings = BuildHeadings(headingJson);
            model.Dates = BuildDates(fromDate, toDate);
            model.Rows = BuildRows(reportData, model.Headings.Count);

            return model;
        }

        // Build date columns
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

        // Build heading list from form configuration JSON
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

        // Recursively read label names from JSON
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

        // Build report rows
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

        // Build left side hierarchy values
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

        // Create unique key for grouping rows
        private string BuildRowKey(List<string> values)
        {
            string key = "";

            foreach (string value in values)
            {
                key = key + value + "|";
            }

            return key;
        }

        // Format result value for report
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