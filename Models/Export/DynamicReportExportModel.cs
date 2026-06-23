using System;
using System.Collections.Generic;

namespace EmployeeAccessSystem.Models
{
    public class DynamicReportExportModel
    {
        public string Title { get; set; }

        public string CategoryName { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<string> Headings { get; set; }

        public List<DateTime> Dates { get; set; }

        public List<DynamicReportExportRow> Rows { get; set; }

        public Dictionary<string, string> DateCreators { get; set; }

        public DynamicReportExportModel()
        {
            Title = string.Empty;
            CategoryName = string.Empty;
            Headings = new List<string>();
            Dates = new List<DateTime>();
            Rows = new List<DynamicReportExportRow>();
            DateCreators = new Dictionary<string, string>();
        }
    }

    public class DynamicReportExportRow
    {
        public List<string> LeftValues { get; set; }

        public Dictionary<string, string> DateValues { get; set; }

        public DynamicReportExportRow()
        {
            LeftValues = new List<string>();
            DateValues = new Dictionary<string, string>();
        }
    }
}