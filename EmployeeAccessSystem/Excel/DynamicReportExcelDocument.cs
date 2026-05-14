using ClosedXML.Excel;
using EmployeeAccessSystem.Models;
using System;
using System.IO;

namespace EmployeeAccessSystem.Excel
{
    public class DynamicReportExcelDocument
    {
        private readonly DynamicReportExportModel _model;

        public DynamicReportExcelDocument(DynamicReportExportModel model)
        {
            _model = model;
        }

        public byte[] Generate()
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet ws = workbook.Worksheets.Add("Report");

                int totalColumns = _model.Headings.Count + _model.Dates.Count;
                int currentRow = 1;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
                ws.Cell(currentRow, 1).Value = _model.Title;
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
                ws.Cell(currentRow, 1).Value = "Product: " + _model.ProductName;
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow++;

                ws.Range(currentRow, 1, currentRow, totalColumns).Merge();
                ws.Cell(currentRow, 1).Value =
                    "From: " + _model.FromDate.ToString("dd/MM/yyyy") +
                    "   To: " + _model.ToDate.ToString("dd/MM/yyyy");
                ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                currentRow += 2;

                int headerRow = currentRow;
                int col = 1;

                foreach (string heading in _model.Headings)
                {
                    ws.Cell(headerRow, col).Value = heading;
                    ws.Cell(headerRow, col).Style.Font.Bold = true;
                    ws.Cell(headerRow, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(headerRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                foreach (DateTime date in _model.Dates)
                {
                    ws.Cell(headerRow, col).Value = date.ToString("dd MMM");
                    ws.Cell(headerRow, col).Style.Font.Bold = true;
                    ws.Cell(headerRow, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                    ws.Cell(headerRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                currentRow++;

                int dataStartRow = currentRow;

                for (int r = 0; r < _model.Rows.Count; r++)
                {
                    col = 1;

                    for (int h = 0; h < _model.Headings.Count; h++)
                    {
                        if (ShouldShowCell(r, h))
                        {
                            int rowSpan = GetRowSpan(r, h);

                            ws.Cell(currentRow, col).Value = _model.Rows[r].LeftValues[h];

                            if (rowSpan > 1)
                            {
                                ws.Range(currentRow, col, currentRow + rowSpan - 1, col).Merge();
                            }

                            ws.Cell(currentRow, col).Style.Alignment.Vertical =
                                XLAlignmentVerticalValues.Center;

                            ws.Cell(currentRow, col).Style.Alignment.Horizontal =
                                XLAlignmentHorizontalValues.Left;
                        }

                        col++;
                    }

                    foreach (DateTime date in _model.Dates)
                    {
                        string key = date.ToString("yyyy-MM-dd");
                        string value = "-";

                        if (_model.Rows[r].DateValues.ContainsKey(key))
                        {
                            value = _model.Rows[r].DateValues[key];
                        }

                        ws.Cell(currentRow, col).Value = value;
                        ws.Cell(currentRow, col).Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        col++;
                    }

                    currentRow++;
                }

                IXLRange usedRange = ws.RangeUsed();

                if (usedRange != null)
                {
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                ws.Columns().AdjustToContents();

                using (MemoryStream stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private bool ShouldShowCell(int rowIndex, int columnIndex)
        {
            if (rowIndex == 0)
            {
                return true;
            }

            for (int i = 0; i <= columnIndex; i++)
            {
                if (_model.Rows[rowIndex].LeftValues[i] !=
                    _model.Rows[rowIndex - 1].LeftValues[i])
                {
                    return true;
                }
            }

            return false;
        }

        private int GetRowSpan(int rowIndex, int columnIndex)
        {
            int count = 1;

            for (int i = rowIndex + 1; i < _model.Rows.Count; i++)
            {
                bool same = true;

                for (int j = 0; j <= columnIndex; j++)
                {
                    if (_model.Rows[i].LeftValues[j] !=
                        _model.Rows[rowIndex].LeftValues[j])
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }
    }
}