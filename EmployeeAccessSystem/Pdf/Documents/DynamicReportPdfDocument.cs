using System;
using EmployeeAccessSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeAccessSystem.Pdf.Documents
{
    public class DynamicReportPdfDocument : IDocument
    {
        private readonly DynamicReportExportModel _model;

        public DynamicReportPdfDocument(DynamicReportExportModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(delegate (PageDescriptor page)
            {
                page.Size(PageSizes.A3.Landscape());
                page.Margin(6);

                page.DefaultTextStyle(delegate (TextStyle style)
                {
                    return style.FontSize(8);
                });

                page.Content().Column(delegate (ColumnDescriptor column)
                {
                    column.Item().AlignCenter().Text(_model.Title ?? "Report").Bold().FontSize(14);

                    column.Item().AlignCenter().Text("Category: " + (_model.CategoryName ?? "-"));

                    column.Item().AlignCenter().Text(
                        "From: " + _model.FromDate.ToString("dd/MM/yyyy") +
                        "   To: " + _model.ToDate.ToString("dd/MM/yyyy")
                    ).Bold();

                    column.Item().PaddingTop(8).Table(delegate (TableDescriptor table)
                    {
                        table.ColumnsDefinition(delegate (TableColumnsDefinitionDescriptor columns)
                        {
                            for (int i = 0; i < _model.Headings.Count; i++)
                            {
                                columns.RelativeColumn(3);
                            }

                            for (int i = 0; i < _model.Dates.Count; i++)
                            {
                                columns.RelativeColumn(1);
                            }
                        });

                        foreach (string heading in _model.Headings)
                        {
                            table.Cell().Element(HeaderCell).Text(heading ?? "-").Bold();
                        }

                        foreach (DateTime date in _model.Dates)
                        {
                            string dateKey = date.ToString("yyyy-MM-dd");
                            string headerText = date.ToString("dd") + "\n" + date.ToString("ddd");
                            if (_model.DateCreators != null && _model.DateCreators.TryGetValue(dateKey, out var creator) && !string.IsNullOrWhiteSpace(creator))
                            {
                                headerText += "\n(" + creator + ")";
                            }
                            table.Cell().Element(HeaderCell).Text(headerText).Bold();
                        }

                        for (int r = 0; r < _model.Rows.Count; r++)
                        {
                            for (int c = 0; c < _model.Headings.Count; c++)
                            {
                                if (ShouldShowCell(r, c))
                                {
                                    table.Cell()
                                        .RowSpan((uint)GetRowSpan(r, c))
                                        .Element(BodyCell)
                                        .Text(_model.Rows[r].LeftValues[c] ?? "-");
                                }
                            }

                            foreach (DateTime date in _model.Dates)
                            {
                                string key = date.ToString("yyyy-MM-dd");
                                string value = "-";

                                if (_model.Rows[r].DateValues != null && _model.Rows[r].DateValues.ContainsKey(key))
                                {
                                    value = _model.Rows[r].DateValues[key];
                                }

                                table.Cell().Element(BodyCell).AlignCenter().Text(value ?? "-");
                            }
                        }
                    });
                });

                page.Footer().AlignRight().Text(delegate (TextDescriptor text)
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }

        private bool ShouldShowCell(int rowIndex, int columnIndex)
        {
            if (rowIndex == 0)
            {
                return true;
            }

            for (int i = 0; i <= columnIndex; i++)
            {
                string currentValue = _model.Rows[rowIndex].LeftValues[i] ?? "";
                string previousValue = _model.Rows[rowIndex - 1].LeftValues[i] ?? "";

                if (currentValue != previousValue)
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
                    string currentValue = _model.Rows[i].LeftValues[j] ?? "";
                    string selectedValue = _model.Rows[rowIndex].LeftValues[j] ?? "";

                    if (currentValue != selectedValue)
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

        private IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Border(1)
                .Padding(2)
                .AlignCenter()
                .AlignMiddle();
        }

        private IContainer BodyCell(IContainer container)
        {
            return container
                .Border(1)
                .Padding(2)
                .AlignMiddle();
        }
    }
}