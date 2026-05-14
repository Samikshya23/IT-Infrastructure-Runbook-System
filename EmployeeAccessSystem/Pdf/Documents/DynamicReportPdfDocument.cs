using EmployeeAccessSystem.Models;
using QuestPDF.Elements.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

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
                page.Margin(12);
                page.DefaultTextStyle(TextStyle.Default.FontSize(8));

                page.Content().Column(delegate (ColumnDescriptor column)
                {
                    column.Item().AlignCenter().Text(_model.Title).Bold().FontSize(14);
                    column.Item().AlignCenter().Text("Product: " + _model.ProductName);

                    column.Item().AlignCenter().Text(
                        "From: " +
                        _model.FromDate.ToString("dd/MM/yyyy") +
                        "   To: " +
                        _model.ToDate.ToString("dd/MM/yyyy")
                    ).Bold();

                    column.Item().PaddingTop(8).Table(delegate (TableDescriptor table)
                    {
                        table.ColumnsDefinition(delegate (TableColumnsDefinitionDescriptor columns)
                        {
                            for (int i = 0; i < _model.Headings.Count; i++)
                            {
                                columns.ConstantColumn(120);
                            }

                            for (int i = 0; i < _model.Dates.Count; i++)
                            {
                                columns.ConstantColumn(45);
                            }
                        });

                        foreach (string heading in _model.Headings)
                        {
                            table.Cell()
                                .Element(delegate (IContainer containerItem)
                                {
                                    return HeaderCell(containerItem);
                                })
                                .Text(heading)
                                .Bold();
                        }

                        foreach (DateTime date in _model.Dates)
                        {
                            table.Cell()
                                .Element(delegate (IContainer containerItem)
                                {
                                    return HeaderCell(containerItem);
                                })
                                .Text(date.ToString("dd") + "\n" + date.ToString("ddd"))
                                .Bold();
                        }

                        for (int r = 0; r < _model.Rows.Count; r++)
                        {
                            for (int c = 0; c < _model.Headings.Count; c++)
                            {
                                if (ShouldShowCell(r, c))
                                {
                                    int rowSpan = GetRowSpan(r, c);

                                    table.Cell()
                                        .RowSpan((uint)rowSpan)
                                        .Element(delegate (IContainer containerItem)
                                        {
                                            return BodyCell(containerItem);
                                        })
                                        .Text(_model.Rows[r].LeftValues[c]);
                                }
                            }

                            foreach (DateTime date in _model.Dates)
                            {
                                string key = date.ToString("yyyy-MM-dd");
                                string value = "-";

                                if (_model.Rows[r].DateValues.ContainsKey(key))
                                {
                                    value = _model.Rows[r].DateValues[key];
                                }

                                table.Cell()
                                    .Element(delegate (IContainer containerItem)
                                    {
                                        return BodyCell(containerItem);
                                    })
                                    .AlignCenter()
                                    .Text(value);
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

        private IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Border(1)
                .Padding(4)
                .AlignCenter()
                .AlignMiddle();
        }

        private IContainer BodyCell(IContainer container)
        {
            return container
                .Border(1)
                .Padding(4)
                .AlignMiddle();
        }
    }
}